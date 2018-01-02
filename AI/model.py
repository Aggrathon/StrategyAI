"""
    Different AI models
"""
import tensorflow as tf
from ops import conv2d_stack

DECAY = 0.98
NETWORK = 'network'
MAX_SEQUENCE = 160
INPUT_WIDTH = 280
INPUT_HEIGHT = 210
INPUT_VARIABLES = 3
COLORS = 3
OUTPUTS = 16


def cnn(data, outputs=None, rewards=None, sequence_lengths=None, batch_size=32, maxlen=MAX_SEQUENCE, reuse=False):
    """
        Create a simple neural network with a cnn
    """
    with tf.variable_scope("shallow_cnn", reuse=reuse):
        data = tf.reshape(data, (batch_size, maxlen, INPUT_WIDTH*INPUT_HEIGHT*COLORS+INPUT_VARIABLES))
        image = data[:, :, :INPUT_HEIGHT*INPUT_WIDTH*COLORS]
        image = tf.reshape(image, (batch_size*maxlen, INPUT_WIDTH, INPUT_HEIGHT, COLORS))
        variables = data[:, :, -INPUT_VARIABLES:]

        #conv
        prev_layer = tf.layers.conv2d(image, 32, 8, 4, name="conv_0")
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
        with tf.control_dependencies(tf.get_collection(tf.GraphKeys.UPDATE_OPS)):
            train = adam.minimize(loss)

        #summaries
        tf.summary.scalar("Value_Loss", loss_value)
        tf.summary.scalar("Value", tf.reduce_mean(tf.abs(value)))

        return train




if __name__ == "__main__":
    #Check for errors without data
    var = tf.placeholder(tf.float32, (32, MAX_SEQUENCE, INPUT_WIDTH*INPUT_HEIGHT*COLORS+INPUT_VARIABLES)) #pylint: disable=C0103
    ph0 = tf.placeholder(tf.float32, (32, MAX_SEQUENCE, OUTPUTS))
    ph1 = tf.placeholder(tf.float32, (32, MAX_SEQUENCE, 1))
    ph2 = tf.placeholder(tf.float32, (32, 1))
    cnn(var, ph0, ph1, ph2, 32, MAX_SEQUENCE, False)
