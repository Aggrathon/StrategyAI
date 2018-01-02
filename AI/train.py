"""
    Train the neural network models with this script
"""

import tensorflow as tf
import numpy as np
from model import cnn, Model
from unityagents import UnityEnvironment

DIR = 'network'
BIN = '../Build/StrategyGame.exe'


def train(epochs=100):
    nn = Model(cnn)
    global_step = tf.train.get_global_step()
    saver = tf.train.Saver()
    env = UnityEnvironment(file_name=BIN)
    replay_buffer = []
    with tf.Session() as sess:
        brainA = env.brains['ExternalA']
        brainB = env.brains['ExternalB']
        try:
            saver.restore(sess, tf.train.latest_checkpoint(DIR))
        except:
            sess.run(tf.initialize_all_variables())
        for e in range(epochs):
            #TODO check levels
            level = 0
            #TODO select models
            nnA = nn
            nnB = nn
            if level < 1:
                env.reset(True, {"Player": 0, "Difficulty": 0.0})
                while not env.global_done:
                    x = []
                    for i in range(len(brainA.agents)):
                        x.append(sess.run(nnA.output, {nnA.input_images: brainA.observations[i], nnA.input_vars: brainA.states[i]}))
                    env.step({'ExternalA': np.argmax(np.asarray(x) + np.random.uniform(0.0, 1.0-level, x.shape), 1) })
                    print(brainA.rewards)
                env.close()
                return


if __name__ == "__main__":
    train() 
