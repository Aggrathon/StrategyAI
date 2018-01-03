"""
    Train the neural network models with this script
"""

import os
import tensorflow as tf
import numpy as np
from model import cnn, Model
from unityagents import UnityEnvironment
from play import play

DIR = 'network'
BIN = '../Build/StrategyGame.exe'
DECAY = 0.98

def setup_session():
    """
        Creates the tf session and unity enviroment
    """
    models = [Model(cnn)]
    saver = tf.train.Saver()
    env = UnityEnvironment(file_name=BIN)
    global_step = tf.train.get_global_step()
    sess = tf.Session()
    try:
        saver.restore(sess, tf.train.latest_checkpoint(DIR))
    except:
        sess.run(tf.global_variables_initializer())
    os.makedirs(DIR, exist_ok=True)
    return sess, saver, global_step, models, env

def train(epochs=100):
    replay_buffer = []
    sess, saver, global_step, models, env = setup_session()
    with sess:
        for e in range(epochs):
            for i in range(100):
                if len(replay_buffer) < 5:
                    while len(replay_buffer) < 20:
                        for nn in models:
                            if nn.level < 1:
                                result, mem_a, _ = play(sess, nn, np.random.choice(models), env, 0, nn.level, True, True)
                                data = _process_data(mem_a)
                                if result > 0.5:
                                    replay_buffer.append(data)
                                    replay_buffer.append(data)
                                    replay_buffer.append(data)
                                    replay_buffer.append(data)
                                    nn.level += 0.2
                                elif np.sum(data[3]) > 0.5:
                                    replay_buffer.append(data)
                                    replay_buffer.append(data)
                                    nn.level += 0.05
                                else:
                                    nn.level *= 0.75
                            else:
                                print("Completed the first difficulty!")
                                saver.save(sess, DIR, global_step)
                                env.close()
                                return
                        print("%d / %d"%(len(replay_buffer), 20), models[0].level)
                    np.random.shuffle(replay_buffer)
                data = replay_buffer.pop()
                for nn in models:
                    nn.train(*data, sess)
            saver.save(sess, os.path.join(DIR, 'model') , global_step)
            print ("Saved epoch", e)
    env.close()

def _process_data(history: list):
    images = []
    variables = []
    actions = []
    reward = []
    prev = np.zeros_like(history[0].rewards, np.float32)
    for m in reversed(history):
        prev = m.rewards + prev*DECAY + np.mean(m.rewards)*(1.0-DECAY)
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
    train()
