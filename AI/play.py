"""
    Script for playing the game
"""
from math import floor
import tensorflow as tf
import numpy as np
from model import Model, BRAIN_A, BRAIN_B
from unityagents import BrainInfo, UnityEnvironment


def _play_step(sess: tf.Session, model: Model, brain: BrainInfo, randomness=0.5):
    action = np.asarray(model.evaluate(sess, brain.observations[0], brain.states, len(brain.agents)))
    if randomness > 0:
        return np.argmax(action + np.random.uniform(0.0, randomness, action.shape), 1)
    else:
        return np.argmax(action, 1)

def _play_randomness(difficulty=0.0, training=False):
    #if not training:
    #    return 0.0
    if difficulty < 1.0:
        return 0.6-difficulty
    return 0.5

def play(sess: tf.Session, nn_a: Model, nn_b: Model, env: UnityEnvironment, players=0.0, difficulty=0.0, training=True, record=True):
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
