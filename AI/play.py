"""
    Script for playing the game
"""
from math import floor
import tensorflow as tf
import numpy as np
from model import Model, BRAIN_A, BRAIN_B
from unityagents import BrainInfo, UnityEnvironment

PLAYERS_AI_VS_AI = 0
PLAYERS_HUMAN_VS_AI = 1
PLAYERS_AI_VS_HUMAN = 2
PLAYERS_HUMAN_VS_HUMAN = 3
PLAYERS_AI_VS_RANDOM = 4
PLAYERS_RANDOM_VS_AI = 5
PLAYERS_HUMAN_VS_RANDOM = 6
PLAYERS_RANDOM_VS_HUMAN = 7
PLAYERS_RANDOM_VS_RANDOM = 8

def _play_step(sess: tf.Session, model: Model, brain: BrainInfo, randomness):
    if randomness >= 1:
        return np.floor(np.random.uniform(0, 17, (len(brain.agents), 1))
    action = np.asarray(model.evaluate(sess, brain.observations[0], brain.states, len(brain.agents)))
    if randomness > 0:
        rnd = np.random.uniform(0.0, randomness, action.shape)
        return np.argmax(action + rnd, 1)
    return np.argmax(action, 1)


def play(sess: tf.Session, nn_a: Model, nn_b: Model, env: UnityEnvironment, players=PLAYERS_AI_VS_AI, randomness=0.0, difficulty=0.0, training=True, record=True):
    """
        Play a game
    """
    brains = env.reset(training, {"Player": players, "Difficulty": floor(difficulty)})
    if record:
        memory_a = []
        memory_b = []
    while not env.global_done:
        out_a = _play_step(sess, nn_a, brains[BRAIN_A], randomness)
        out_b = _play_step(sess, nn_b, brains[BRAIN_B], randomness) if nn_b else []
        brains = env.step({BRAIN_A: out_a, BRAIN_B: out_b})
        if record:
            memory_a.append(brains[BRAIN_A])
            if nn_b:
                memory_b.append(brains[BRAIN_B])
    reward = (np.sum(brains[BRAIN_A].rewards) - np.sum(brains[BRAIN_B].rewards)) \
        / (len(brains[BRAIN_A].rewards) + len(brains[BRAIN_A].rewards))
    if record:
        return reward, memory_a, memory_b
    else:
        return reward
