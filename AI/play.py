"""
    Script for playing the game
"""
import tensorflow as tf
import numpy as np
from model import Model, BRAIN_A, BRAIN_B
from unityagents import BrainInfo, UnityEnvironment

PLAYERS_AI_1 = 0
PLAYERS_AI_2 = 1
PLAYERS_HUMAN_1 = 2
PLAYERS_HUMAN_2 = 3
PLAYERS_RANDOM_1 = 4
PLAYERS_RANDOM_2 = 5

def _play_step(sess: tf.Session, model: Model, brain: BrainInfo, randomness):
    if randomness >= 1:
        return np.floor(np.random.uniform(0, 17, (len(brain.agents),)))
    action = np.asarray(model.evaluate(sess, brain.observations[0], brain.states, len(brain.agents)))
    if randomness > 0:
        rnd = np.random.uniform(0.0, randomness, action.shape)
        return np.argmax(action*(1-randomness) + rnd*randomness, 1)
    return np.argmax(action, 1)


def play(sess: tf.Session, nn_a: Model, nn_b: Model, env: UnityEnvironment, player_one=PLAYERS_AI_1, player_two=PLAYERS_AI_2, randomness=0.0, difficulty=0, training=True, record=True):
    """
        Play a game
    """
    brains = env.reset(training, {"PlayerOne": player_one, "PlayerTwo": player_two, "Difficulty": difficulty})
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
    if nn_b:
        ra = np.max(brains[BRAIN_A].states[:, 3])
        rb = np.max(brains[BRAIN_B].states[:, 3])
        if ra >= 1.0:
            reward = 1.0
        elif rb >= 1.0:
            reward = -1.0
        else:
            reward = ra - rb
    else:
        reward = np.max(brains[BRAIN_A].states[:, 3])
    if record:
        return reward, memory_a, memory_b
    else:
        return reward
