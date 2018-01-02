"""
    Different AI models
"""
import tensorflow as tf
from ops import conv2d_stack

BATCH_SIZE = 32
MAX_SEQUENCE = 160
INPUT_WIDTH = 280
INPUT_HEIGHT = 210
INPUT_VARIABLES = 3
COLORS = 3
OUTPUTS = 16
DECAY = 0.98


class Model():
    """
        Wrapper for the different model functions
    """
    def __init__(self, model_fn, batch_size=BATCH_SIZE):
        self.level = 0.0
        self.input_images = tf.placeholder(tf.float32)
        self.input_vars = tf.placeholder(tf.float32)
        self.output_vars = tf.placeholder(tf.float32, (batch_size, MAX_SEQUENCE, OUTPUTS))
        self.output_rewards = tf.placeholder(tf.float32, (batch_size, MAX_SEQUENCE, 1))
        self.trainer = model_fn(self.input_images, self.input_vars, self.output_vars, self.output_rewards, batch_size=batch_size)
        self.output = model_fn(self.input_images, self.input_vars, batch_size=1, maxlen=1, reuse=True)


def cnn(images, variables, outputs=None, rewards=None, sequence_lengths=None, batch_size=32, maxlen=MAX_SEQUENCE, reuse=False):
    """
        Create a simple neural network with a cnn
    """
    with tf.variable_scope("shallow_cnn", reuse=reuse):
        images = tf.reshape(images, (batch_size*maxlen, INPUT_WIDTH, INPUT_HEIGHT, COLORS))
        variables = tf.reshape(variables, (batch_size, maxlen, INPUT_VARIABLES))

        #conv
        prev_layer = tf.layers.conv2d(images, 32, 8, 4, name="conv_0")
        prev_layer = conv2d_stack(prev_layer, 24, pool=True, name="conv_1")
        prev_layer = conv2d_stack(prev_layer, 32, name="conv_2")
        prev_layer = conv2d_stack(prev_layer, 40, name="conv_3")
        #print(prev_layer.get_shape())

        #combine
        prev_layer = tf.reshape(prev_layer, (batch_size, maxlen, -1))
        prev_layer = tf.concat((prev_layer, variables), 2)
        prev_layer = tf.layers.batch_normalization(prev_layer)

        #dense
        prev_layer = tf.layers.dense(prev_layer, 1024, tf.nn.relu, name='relu_1')
        prev_layer = tf.layers.dropout(prev_layer, 0.3)
        prev_layer = tf.layers.dense(prev_layer, 256, tf.nn.relu, name='relu_2')
        prev_layer = tf.layers.dropout(prev_layer, 0.3)
        prev_layer = tf.layers.dense(prev_layer, 64, tf.nn.relu, name='relu_3')

        #output
        logits = tf.layers.dense(prev_layer, OUTPUTS, name='logits')
        output = tf.nn.softmax(logits, name='output')
        if outputs is None or rewards is None or sequence_lengths is None:
            return output

        #value
        prev_layer = tf.layers.dense(prev_layer, 16, tf.nn.relu, name="relu_4")
        value = tf.layers.dense(prev_layer, 1, name="value")

        #training
        loss_value = tf.losses.mean_squared_error(rewards, value)
        loss_action = tf.losses.softmax_cross_entropy(outputs, logits)
        loss = loss_value + loss_action*rewards*tf.abs(value-rewards)
        adam = tf.train.AdamOptimizer(1e-4)
        global_step = tf.train.get_or_create_global_step()
        with tf.control_dependencies(tf.get_collection(tf.GraphKeys.UPDATE_OPS)):
            train = adam.minimize(loss, global_step=global_step)

        #summaries
        tf.summary.scalar("Value_Loss", loss_value)
        tf.summary.scalar("Value", tf.reduce_mean(tf.abs(value)))

        return train


if __name__ == "__main__":
    #Check for errors without data
    Model(cnn)
