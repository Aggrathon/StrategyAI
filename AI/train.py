"""
    Train the neural network models with this script
"""

import os
import tensorflow as tf
import numpy as np
from model import cnn, Model
from unityagents import UnityEnvironment
import play

DIR = 'network'
BIN = '../Build/StrategyGame.exe'
MAX_LEVEL = 3
LEVEL_SINGLE_PLAYER = [0, 1]
BUFFER_SIZE = 15
REWARD_DECAY = 0.98

class Trainer():
    """
        Class for training neural network models
        Use 'with' for automatic cleanup
    """
    def __init__(self, network_directory=DIR):
        self.models = [Model(cnn, 'cnn_1'), Model(cnn, 'cnn_2')]
        self.saver = tf.train.Saver()
        self.env = UnityEnvironment(file_name=BIN)
        self.global_step = tf.train.get_global_step()
        self.summary = tf.summary.merge_all()
        self.sess = tf.Session()
        self.summary_writer = tf.summary.FileWriter(network_directory, self.sess.graph)
        try:
            self.saver.restore(self.sess, tf.train.latest_checkpoint(network_directory))
        except: #pylint ignore=0702
            self.sess.run(tf.global_variables_initializer())
        os.makedirs(DIR, exist_ok=True)
        self.replay_buffer = []
        self.replay_strength = []

    def evaluate(self, nn: Model):
        """
            Evaluates the training of a model
        """
        result = 0.0
        for i in range(4):
            if nn.level in LEVEL_SINGLE_PLAYER:
                res, mem, _ = play.play(self.sess, nn, None, self.env, play.PLAYERS_AI_1, play.PLAYERS_AI_1, 0, nn.level, True, True)
            elif i%2 == 0:
                res, mem, _ = play.play(self.sess, nn, None, self.env, play.PLAYERS_AI_1, play.PLAYERS_RANDOM_1, 0, nn.level, True, True)
            else:
                res, mem, _ = play.play(self.sess, nn, None, self.env, play.PLAYERS_RANDOM_1, play.PLAYERS_AI_1, 0, nn.level, True, True)
            self._add_to_replay_buffer(mem, res)
            if res <= 0.1:
                return 0.0
            result += res
        return result / 4.0

    def find_levels(self):
        """
            Finds the appropriate level of the models
        """
        print("Checking for level improvements")
        for nn in self.models:
            while True:
                result = self.evaluate(nn)
                if result < 0.1:
                    nn.randomnesss += 0.1
                    if nn.randomnesss > 0.7:
                        nn.level = max(0, nn.level-1)
                        nn.randomnesss = 4 / (7 + nn.level) + 0.25
                    break
                elif result < 0.9:
                    nn.randomnesss = max(0.15, nn.randomnesss-0.15)
                    break
                else:
                    nn.level += 1
                    nn.randomnesss = 4 / (7 + nn.level) + 0.2
                    if nn.level >= MAX_LEVEL:
                        break
            print('\n'+nn.name+':', 'level %d, randomness %.2f, result %.2f\n'%(nn.level, nn.randomnesss, result))

    def _fill_replay_buffer(self):
        not_added = True
        while not_added:
            for nn in self.models:
                if nn.level in LEVEL_SINGLE_PLAYER:
                    result, mem_a, _ = play.play(self.sess, nn, None, self.env, play.PLAYERS_AI_1, play.PLAYERS_AI_1, nn.randomnesss, nn.level, True, True)
                    if result > 0.8:
                        not_added = False
                        self._add_to_replay_buffer(mem_a, result)
                else:
                    result, mem_a, mem_b = play.play(self.sess, nn, np.random.choice(self.models), self.env, play.PLAYERS_AI_1, play.PLAYERS_AI_2, nn.randomnesss, nn.level, True, True)
                    if result != 0:
                        not_added = False
                    self._add_to_replay_buffer(mem_a, result)
                    self._add_to_replay_buffer(mem_b, -result)
                print("Game Result:", result)

    def _add_to_replay_buffer(self, data, result):
        if result == 0:
            return
        data = _process_data(data, result)
        self.replay_buffer.append(data)
        self.replay_strength.append(max(1, min(5, result*6)))

    def close(self):
        """
            Close all sessions and connections
        """
        self.sess.close()
        self.env.close()

    def __enter__(self):
        return self
    def __exit__(self, exc_type, exc_value, traceback):
        self.close()

    def train(self, epochs=100):
        """
            Train the models for a number of epochs.
            An epoch is 100 iterations long.
        """
        for e in range(epochs):
            for j in range(10):
                self.find_levels()
                print("Training\n")
                for i in range(50):
                    if len(self.replay_buffer) < BUFFER_SIZE:
                        self._fill_replay_buffer()
                    index = np.random.randint(len(self.replay_buffer))
                    data = self.replay_buffer[index]
                    self.replay_strength[index] -= 1
                    if self.replay_strength[index] <= 0:
                        self.replay_buffer[index] = self.replay_buffer[-1]
                        self.replay_strength[index] = self.replay_strength[-1]
                        del self.replay_buffer[-1]
                        del self.replay_strength[-1]
                    fd = dict()
                    trainers = []
                    for nn in self.models:
                        nn.append_feed_dict(trainers, fd, *data)
                    if i == 0:
                        trainers.append(self.summary)
                        trainers.append(self.global_step)
                        self.summary_writer.add_summary(*self.sess.run(trainers, fd)[-2:] )
                    else:
                        self.sess.run(trainers, fd)
            self.saver.save(self.sess, os.path.join(DIR, 'model'), self.global_step)
            print("Saved epoch", e)


def _process_data(history: list, result):
    images = []
    variables = []
    actions = []
    reward = []
    prev = np.zeros_like(history[-1].rewards)
    for m in reversed(history):
        tmp = m.rewards
        m.rewards = prev
        prev = tmp + prev*REWARD_DECAY + np.mean(tmp)*0.05
    for i in range(len(history)-1):
        old = history[i]
        new = history[i+1]
        for j in range(len(old.agents)):
            if not old.local_done[j]:
                images.append(old.observations[0][j])
                variables.append(old.states[j])
                actions.append(new.previous_actions[j])
                reward.append(new.rewards[j])
    return (images, variables, actions, reward, result, len(images))


if __name__ == "__main__":
    with Trainer() as t:
        t.train()
