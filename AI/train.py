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
DECAY = 0.975
MAX_LEVEL = 3

class Trainer():
    def __init__(self, network_directory=DIR):
        self.models = [Model(cnn)]
        self.saver = tf.train.Saver()
        self.env = UnityEnvironment(file_name=BIN)
        self.global_step = tf.train.get_global_step()
        self.sess = tf.Session()
        try:
            self.saver.restore(self.sess, tf.train.latest_checkpoint(network_directory))
        except:
            self.sess.run(tf.global_variables_initializer())
        os.makedirs(DIR, exist_ok=True)
        self.replay_buffer = []

    def evaluate(self, nn: Model):
        """
            Evaluates the training of a model
        """
        res, mem, _ = play.play(self.sess, nn, None, self.env, play.PLAYERS_AI_VS_RANDOM, 0, nn.level, True, True)
        self._add_to_replay_buffer(mem, res)
        if res < 0.0:
            return False
        res, _, mem = play.play(self.sess, nn, None, self.env, play.PLAYERS_RANDOM_VS_AI, 0, nn.level, True, True)
        self._add_to_replay_buffer(mem, -res)
        if res < 0.0:
            return False
        return True

    def find_levels(self):
        """
            Finds the appropriate level of the models
        """
        for nn in self.models:
            if self.evaluate(nn):
                nn.randomnesss = max(0.1, nn.randomnesss-0.1)
                nn.level += 1
                while self.evaluate(nn) and nn.level <= MAX_LEVEL:
                    nn.randomnesss = 2 / (3 + nn.level) + 0.2
                    nn.level += 1
                nn.level -= 1
            else:
                nn.randomnesss = min(1.0, nn.randomnesss+0.1)

    def _fill_replay_buffer(self):
        while len(self.replay_buffer) < 40:
            for nn in self.models:
                result, mem_a, mem_b = play(self.sess, nn, np.random.choice(self.models), self.env, nn.randomnesss, nn.level, True, True)
                self._add_to_replay_buffer(mem_a, result)
                if nn.level > 1:
                    self._add_to_replay_buffer(mem_b, -result)
            #TODO
            print("Filling replay buffer: %d / %d"%(len(self.replay_buffer), 40))
        np.random.shuffle(self.replay_buffer)

    def _add_to_replay_buffer(self, data, result):
        data = _process_data(data)
        if result > 0.5:
            self.replay_buffer.append(data)
            self.replay_buffer.append(data)
        if result > 0.3:
            self.replay_buffer.append(data)
        if result > 0.0:
            self.replay_buffer.append(data)
        self.replay_buffer.append(data)

    def close(self):
        self.sess.close()
        self.env.close()

    def __enter__(self):
        return self
    def __exit__(self, exc_type, exc_value, traceback):
        self.close()

    def train(self, epochs=100):
        for e in range(epochs):
            self.find_levels()
            for i in range(100):
                if len(self.replay_buffer) < 5:
                    self._fill_replay_buffer()
                data = self.replay_buffer.pop()
                for nn in self.models:
                    nn.train(*data, self.sess)
            self.saver.save(self.sess, os.path.join(DIR, 'model') , self.global_step)
            print ("Saved epoch", e)


def _process_data(history: list):
    images = []
    variables = []
    actions = []
    reward = []
    if np.mean(history[-1].rewards) < 0.9 and np.mean(history[-1].rewards) > -0.9:
        history[-1].rewards = np.zeros_like(history[-1], np.float32)
    prev = history[-1].rewards
    for m in reversed(history[:-1]):
        prev = m.rewards + prev*DECAY + np.mean(m.rewards)*(1.0-(1.0-DECAY)*20)
        m.rewards = prev
    for i in range(len(history)-1):
        old = history[i]
        new = history[i+1]
        for j in range(len(old.agents)):
            if not old.local_done[j]:
                images.append(old.observations[0][j])
                variables.append(old.states[j])
                actions.append(new.previous_actions[j])
                reward.append(new.rewards[j])
    return (images, variables, actions, reward, len(images))


if __name__ == "__main__":
    with Trainer() as t:
        t.train()
