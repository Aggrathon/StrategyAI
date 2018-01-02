"""
    Train the neural network models with this script
"""

import tensorflow as tf
import numpy as np
from model import cnn, Model
from unityagents import UnityEnvironment

DIR = 'network'
BIN = '../Build/StrategyGame.exe'
BRAIN_A = 'ExternalA'
BRAIN_B = 'ExternalB'


def train(epochs=100):
    nn = Model(cnn)
    global_step = tf.train.get_global_step()
    saver = tf.train.Saver()
    env = UnityEnvironment(file_name=BIN)
    replay_buffer = []
    with tf.Session() as sess:
        try:
            saver.restore(sess, tf.train.latest_checkpoint(DIR))
        except:
            sess.run(tf.global_variables_initializer())
        for e in range(epochs):
            #TODO check levels
            level = 0
            #TODO select models
            nnA = nn
            nnB = nn
            if level < 1:
                brains = env.reset(False, {"Player": 0, "Difficulty": 0.0})
                while not env.global_done:
                    out = []
                    for i in range(len(brains[BRAIN_A].agents)):
                        x = np.asarray(sess.run(nnA.output, {nnA.input_images: brains[BRAIN_A].observations[0][i], nnA.input_vars: brains[BRAIN_A].states[i]}))
                        out.append(np.argmax(np.asarray(x) + np.random.uniform(0.0, 1.0-level, x.shape)))
                    brains = env.step({BRAIN_A: out, BRAIN_B: [-1, -1, -1, -1] })
                    print(brains[BRAIN_A].rewards)
                env.close()
                return


if __name__ == "__main__":
    train() 
