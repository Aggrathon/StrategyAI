"""
    Neural network operators for the models
"""

import tensorflow as tf


def conv2d_stack(image, filters=32, stride=2, pool=False, name="conv2d_stack"):
    """
        Creates a conv2d layer with different kernel sizes concatenated
    """
    with tf.variable_scope(name):
        output = tf.concat((
            tf.layers.conv2d(image, filters, 3, 1 if pool else stride, 'same', name="conv_3"),
            tf.layers.conv2d(image, filters, 5, 1 if pool else stride, 'same', name="conv_5"),
            tf.layers.conv2d(image, filters, 7, 1 if pool else stride, 'same', name="conv_7")
        ), 3, "stack")
        if pool:
            output = tf.layers.max_pooling2d(output, 2, stride, name='pool')
        return output
