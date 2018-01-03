"""
    Train the neural network models with this script
"""

from math import floor
import tensorflow as tf
import numpy as np
from model import cnn, Model
from unityagents import UnityEnvironment, BrainInfo

DIR = 'network'
BIN = '../Build/StrategyGame.exe'
BRAIN_A = 'ExternalA'
BRAIN_B = 'ExternalB'

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
    return sess, saver, global_step, models, env

def train(epochs=100):
    replay_buffer = []
    sess, saver, global_step, models, env = setup_session()
    nn = models[0]
    with sess:
        for e in range(epochs):
            #TODO check levels
            level = 0
            #TODO select models
            if level < 1:
                result, mem_a, _ = play(sess, nn, nn, env, 0, level, True, True)
                replay_buffer.append(mem_a)
                env.close()
                return
            saver.save(sess, DIR, global_step)


def _play_step(sess: tf.Session, model: Model, brain: BrainInfo, randomness=0.5):
    out = []
    for i, _ in enumerate(brain.agents):
        if brain.local_done[i]:
            out.append(-1)
            continue
        action = np.asarray(sess.run(model.output, {model.input_images: brain.observations[0][i], model.input_vars: brain.states[i]}))
        if randomness > 0:
            out.append(np.argmax(action + np.random.uniform(0.0, randomness, action.shape)))
        else:
            out.append(np.argmax(action))
    return out

def _play_randomness(difficulty=0.0, training=False):
    #if not training:
    #    return 0.0
    if difficulty < 1.0:
        return 1.0-difficulty
    return 0.5

def _play_record(memory: list, old: BrainInfo, new: BrainInfo):
    if old.agents:
        for i in range(len(old.agents)):
            image = old.observations[0][i]
            state = old.states[i]
            action = new.previous_actions[i]
            reward = new.rewards[i]
            memory.append((image, state, action, reward))

def play(sess: tf.Session, nn_a: Model, nn_b: Model, env: UnityEnvironment, players=0, difficulty=0, training=False, record=True):
    """
        Play a game
    """
    brains = env.reset(training, {"Player": players, "Difficulty": floor(difficulty)})
    if record:
        memory_a = []
        memory_b = []
    while not env.global_done:
        rnd = _play_randomness(difficulty, training)
        out_a = _play_step(sess, nn_a, brains[BRAIN_A], rnd)
        out_b = _play_step(sess, nn_b, brains[BRAIN_B], rnd)
        brains_old = brains
        brains = env.step({BRAIN_A: out_a, BRAIN_B: out_b})
        if record:
            _play_record(memory_a, brains_old[BRAIN_A], brains[BRAIN_A])
            _play_record(memory_b, brains_old[BRAIN_B], brains[BRAIN_B])
    print(brains[BRAIN_A], brains[BRAIN_A].agents, brains[BRAIN_A].rewards)
    if record:
        return np.mean(brains[BRAIN_A].rewards), memory_a, memory_b
    else:
        return np.mean(brains[BRAIN_A].rewards)


if __name__ == "__main__":
    train()
