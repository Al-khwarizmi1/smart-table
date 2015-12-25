
#define echoPin 7 // Echo Pin
#define trigPin 8 // Trigger Pin

String command;   // for incoming serial data

long duration, distance; // Duration used to calculate distance

void setup()
{
    Serial.begin(9600);
    pinMode(trigPin, OUTPUT);
    pinMode(echoPin, INPUT);
}

void loop()
{
    if (Serial.available() > 0)
    {
        command = Serial.readString();
        //Serial.println(command);
        if (command == "1")
        {
            Serial.println(getHeight());
        }
    }
}

long getHeight()
{
    digitalWrite(trigPin, LOW);
    delayMicroseconds(2);
    digitalWrite(trigPin, HIGH);
    delayMicroseconds(10);
    digitalWrite(trigPin, LOW);
    duration = pulseIn(echoPin, HIGH);
    return duration / 58.2;
}


