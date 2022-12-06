import zmq, cv2
import numpy as np

from keras.models import load_model
from utils import preprocess_image

model = load_model('model.h5')

context = zmq.Context()
socket = context.socket(zmq.REP)
socket.bind("tcp://*:5555")

(CROP_HEIGHT, CROP_WIDTH) = (80, 56)

image = [[0, 0, 0]]
count = 0
steering_angle = 0

print("Waiting for image data...\n")

while True:
    bytes_received = socket.recv()
    path_string = bytes_received.decode ("utf-8")
    
    img = cv2.imread (path_string)
    image[0][count] = preprocess_image (img)
    count += 1

    if (count % 3 == 0):
        image = np.array (image)
        print (image[0].shape)
        for i in range (3):
            image[0][i] = np.array(image[0][i]).reshape (-1, CROP_WIDTH, CROP_HEIGHT, 3)
        image = image /255.0

        steering_angle = model.predict(image)[0][0]
        count = 0
        print(steering_angle*6)

    msg = bytes(str(steering_angle), encoding= 'utf-8')
    socket.send(msg)
        