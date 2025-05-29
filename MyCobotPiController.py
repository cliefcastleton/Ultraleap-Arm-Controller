from pymycobot.mycobot import MyCobot
from pymycobot.genre import Coord
import socket

myCobot = MyCobot('/dev/ttyAMA0', 115200)
myCobot.power_on()
myCobot.set_gripper_value(100, 100) #50 is closed, 100 is open
myCobot.send_angles([0, 50, -95, 50, 0, 0], 15)


s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
s.connect(('8.8.8.8', 1))
host = s.getsockname()[0]
port = 0

portInput = ""

while portInput != "5000" and portInput != "8080":
    portInput = input("Enter the network port to use ('5000' or '8080'):\n")

    if portInput == "5000":
        port = 5000
    elif portInput == "8080":
        port = 8080
    else:
        print("Not a valid port! Please select either '5000' or '8080'")


print("waiting for Unity connection... Please enter '" + host + "' as IP and '" + str(port) + "' as port in the Unity application.")


def ProcessAngleInput(input):
    return input.split(",")

def ServerProgram():
    serverSocket = socket.socket()
    serverSocket.bind((host, port))
    serverSocket.listen(2)

    angleAcceptableRange = 2

    #print(host)

    connection, clientAddress = serverSocket.accept()
    
    pose = "OpenPalm"
    angles = [0, 50, -95, 50, 0, 0]

    print("Unity connected from: " + str(clientAddress))

    while True:
        clientMessage = connection.recv(1024).decode()

        currentCoords = myCobot.get_coords()


        # if not clientMessage:
            # break
           
        print("Client" + clientMessage)
        
        try:
            pose = clientMessage.split(';')[0]
            # print(pose)
            # print(clientMessage.split(";"))
            # print(clientMessage.split(";")[1])
            angles = ProcessAngleInput(clientMessage.split(";")[1])
        except Exception as e:
            print(e)
            connection, clientAddress = serverSocket.accept()

        #print("Pose: " + pose + ", Angles: J1: " + angles[0] + ", J2: " + angles[1] + ", J3: " + angles[2] + ", J4: " + angles[3])


        if pose == "Fist" or pose == "Point":
            myCobot.set_gripper_value(50, 100) #50 is closed, 100 is open
            myCobot.set_color(255, 100, 255)
        else:
            myCobot.set_gripper_value(65, 100) #50 is closed, 100 is open
            myCobot.set_color(75, 255, 75)
        
        currentAngles = myCobot.get_angles()
        print(currentAngles)

        angles = [float(angles[0]), float(angles[1]), float(angles[2]), float(angles[3]), 180, 0]

        anglesValid = False

        try:
            angles[0]
            angles[1]
            angles[2]
            angles[3]

            anglesValid = True

        except:
            print("Out of bounds error!")
        

        if anglesValid:
            if angles[0] - currentAngles[0] > angleAcceptableRange or angles[1] - currentAngles[1] > angleAcceptableRange or angles[2] - currentAngles[2] > angleAcceptableRange or angles[3] - currentAngles[3] > angleAcceptableRange:
            #print(angles)

                try:
                    print(angles)
                    myCobot.send_angles(angles, 85) # 85
                except:
                    print("Issue setting angles!")

        

        #angles = [float(angles[0]), 0, 0, 0, 0, 0]

        

        serverMessage = "Pong"
        # connection.send(serverMessage.encode())
    print("Unity disconnected!")

    myCobot.send_angles([0, 35, -70, 35, 0, 0], 15)
    connection.close()
    
if __name__ == '__main__':
    ServerProgram()
