"""
    Different AI models
"""
import tensorflow as tf
from ops import conv2d_stack

INPUT_WIDTH = 280
INPUT_HEIGHT = 200
INPUT_VARIABLES = 4
INPUT_COLORS = 3
OUTPUTS = 16
BRAIN_A = 'ExternalA'
BRAIN_B = 'ExternalB'
LEARNING_RATE = 5e-5


class Model():
    """
        Wrapper for the different model functions
    """
    def __init__(self, model_fn, name):
        self.name = name
        self.level = 0
        self.randomnesss = 0.6
        self.input_images = tf.placeholder(tf.float32)
        self.input_vars = tf.placeholder(tf.float32)
        self.output_vars = tf.placeholder(tf.float32)
        self.sequence_length = tf.placeholder(tf.int32)
        self.output_rewards = tf.placeholder(tf.float32)
        self.output, self.trainer = model_fn(self.input_images, self.input_vars, self.output_vars, self.output_rewards, self.sequence_length)

    def append_feed_dict(self, trainers: list, feed_dict: dict, images, variables, outputs, rewards, length):
        """
            Fill a feed dict for one iteration of reinforcement learning
        """
        trainers.append(self.trainer)
        feed_dict[self.input_images] = images
        feed_dict[self.input_vars] = variables
        feed_dict[self.sequence_length] = length
        feed_dict[self.output_vars] = outputs
        feed_dict[self.output_rewards] = rewards

    def evaluate(self, sess: tf.Session, image, variables, length=1):
        """
            Make a decision for a game
        """ 
        return sess.run(self.output, {
            self.input_images: image,
            self.input_vars: variables,
            self.sequence_length: length
        })


def cnn(images, variables, outputs=None, rewards=None, sequence_length=1, reuse=False):
    """
        Create a simple neural network with a cnn
    """
    with tf.variable_scope("shallow_cnn", reuse=reuse):
        images = tf.reshape(images, (sequence_length, INPUT_HEIGHT, INPUT_WIDTH, INPUT_COLORS))
        variables = tf.reshape(variables, (sequence_length, INPUT_VARIABLES))

        #conv
        prev_layer = tf.layers.conv2d(images, 32, 8, 4, name="conv_0")
        prev_layer = conv2d_stack(prev_layer, 24, pool=True, name="conv_1")
        prev_layer = conv2d_stack(prev_layer, 32, name="conv_2")
        prev_layer = conv2d_stack(prev_layer, 40, name="conv_3")
        #print(prev_layer.get_shape())

        #combine
        prev_layer = tf.reshape(prev_layer, (sequence_length, 6*9*120))
        prev_layer = tf.concat((prev_layer, variables), 1)
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
        if outputs is None or rewards is None:
            return output

        #value
        value = tf.layers.dense(prev_layer, 16, tf.nn.relu, name="relu_value")
        value = tf.layers.dense(value, 1, name="value")
        #prediction
        prediction = tf.layers.dense(prev_layer, 16, tf.nn.relu, name="relu_prediction")
        prediction = tf.layers.dense(prediction, 1, name="prediction")

        #training
        loss_value = tf.losses.mean_squared_error(rewards[:-1], value[:-1])
        loss_pred = tf.reduce_mean(tf.square(prediction-rewards[-1]))
        loss_action = tf.losses.softmax_cross_entropy(tf.one_hot(tf.to_int32(outputs), OUTPUTS), logits)
        loss = loss_value*0.5 + loss_pred*0.5 + loss_action*tf.nn.leaky_relu(rewards, 0.1)*tf.abs(value-rewards)
        adam = tf.train.AdamOptimizer(LEARNING_RATE)
        global_step = tf.train.get_or_create_global_step()
        with tf.control_dependencies(tf.get_collection(tf.GraphKeys.UPDATE_OPS)):
            train = adam.minimize(loss, global_step=global_step)

        #summaries
        tf.summary.scalar("Value_Loss", loss_value)
        tf.summary.scalar("Prediction_Loss", loss_pred)
        tf.summary.scalar("Action_Loss", loss_action)
        tf.summary.histogram("Action", tf.reduce_mean(output, 1))

        return output, train


if __name__ == "__main__":
    #Check for errors without data
    Model(cnn, 'cnn')
