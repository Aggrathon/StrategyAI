"""
    Script for playing the game
"""
from math import floor
import tensorflow as tf
import numpy as np
from model import Model, BRAIN_A, BRAIN_B
from unityagents import BrainInfo, UnityEnvironment


def _play_step(sess: tf.Session, model: Model, brain: BrainInfo, difficulty, training):
    action = np.asarray(model.evaluate(sess, brain.observations[0], brain.states, len(brain.agents)))
    return _play_randomness(action, difficulty, training)
        

def _play_randomness(data, difficulty=0.0, training=False):
    #if not training:
    #    return np.argmax(data, 1)
    if difficulty < 1.0:
        shape = (data.shape[0], data.shape[1]//2)
        limit = 0.6-difficulty
        rnd = np.concatenate((np.random.uniform(0.0, limit, shape), np.random.uniform(0.0, limit*0.5, shape)), 1)
        return np.argmax(data + rnd, 1)
    return np.argmax(data, 1)

def play(sess: tf.Session, nn_a: Model, nn_b: Model, env: UnityEnvironment, players=0.0, difficulty=0.0, training=True, record=True):
    """
        Play a game
    """
    brains = env.reset(training, {"Player": players, "Difficulty": floor(difficulty)})
    if record:
        memory_a = []
        memory_b = []
    while not env.global_done:
        out_a = _play_step(sess, nn_a, brains[BRAIN_A], difficulty, training)
        out_b = _play_step(sess, nn_b, brains[BRAIN_B], difficulty, training)
        if record:
            memory_a.append(brains[BRAIN_A])
            memory_b.append(brains[BRAIN_B])
        brains = env.step({BRAIN_A: out_a, BRAIN_B: out_b})
    reward = (np.sum(brains[BRAIN_A].rewards) - np.sum(brains[BRAIN_B].rewards)) \
        / (len(brains[BRAIN_A].rewards) + len(brains[BRAIN_A].rewards))
    if record:
        return reward, memory_a, memory_b
    else:
        return reward
