#include <Servo.h>

const int enA = 3;
const int enB = 5;
const int in1 = 2;
const int in2 = 4;
const int in3 = 7;
const int in4 = 8;
const int ledPin = 6;
const int buzzerPin = 9;
const int echoPin = 12;
const int trigPin = 13;
const int vServoPin = 10;
const int hServoPin = 11;

Servo vServo, hServo;

String inString = "";     
const char commandDelimiter = '|';
const char valueDelimiter = ':';

void setup() {
  pinMode(enA, OUTPUT);
  pinMode(enB, OUTPUT);
  pinMode(in1, OUTPUT);
  pinMode(in2, OUTPUT);
  pinMode(in3, OUTPUT);
  pinMode(in4, OUTPUT);
  pinMode(ledPin, OUTPUT);
  pinMode(buzzerPin, OUTPUT);
  pinMode(echoPin, INPUT);
  pinMode(trigPin, OUTPUT);
  pinMode(vServoPin, OUTPUT);
  pinMode(hServoPin, OUTPUT);
  
  vServo.attach(vServoPin);
  hServo.attach(hServoPin);
  
  Serial.begin(9600);
}

void loop() {
  
  while (Serial.available() == 0);
  
  //Serial.println("Byte available");
  
  // reads till it finds the symbol '|' and clears anything read
  // returns the full string if no '|' can be found
  String cmd = Serial.readStringUntil(commandDelimiter);
  cmd.trim();  
  Serial.println("Main Cmd: " + cmd);
  
  int firstValue, secondValue = 0;
  
  String tempString = cmd.substring(1, cmd.length() - 1);
  
  Serial.print("Temp string : ");
  Serial.println(tempString);
  
  int valueDelimiterIndex = tempString.indexOf(':');
  if(valueDelimiterIndex != -1){
    firstValue = getValue(tempString, valueDelimiter, 0).toInt();
    secondValue = getValue(tempString, valueDelimiter, 1).toInt();
  }
  else
    firstValue = cmd.substring(1, cmd.length()).toInt();
    
  Serial.print("First Value : ");
  Serial.println(firstValue);
  
  Serial.print("Second Value : ");
  Serial.println(secondValue);
  
  
  char action = cmd.charAt(0);
  switch(action){
    case('W'):
    {
      // Forward
      motorControl(0, 1, firstValue);
      motorControl(1, 1, firstValue);
      break;
    }
    case('S'):
    {
      // Backward
      motorControl(0, 2, firstValue);
      motorControl(1, 2, firstValue);
      break;
    }
    case('A'):
    {
      // Left
      motorControl(0, 2, firstValue);
      motorControl(1, 1, firstValue);
      break;
    }
    case('D'):
    {
      // Right
      motorControl(0, 1, firstValue);
      motorControl(1, 2, firstValue);
      break;
    }
    case('V'):
    {
      // Vertical Servo
      vServo.write(firstValue);
      break;
    }
    case('H'):
    {
      // Horizontal Servo
      hServo.write(firstValue);
      break;
    }
    case('B'):
    {
      // Buzzer
      tone(buzzerPin, firstValue, secondValue * 1.3);
      break;
    }
    case('L'):
    {
      // Light
      int tempVal = map(firstValue, 0, 100, 0, 255);
      analogWrite(ledPin, tempVal);
      break;
    }
    default:
      digitalWrite(enA, LOW);
      digitalWrite(enB, LOW);
      break;
  }
}

void motorControl(int motorName, int operation, int motorSpeed)
{
  
  motorSpeed = map(motorSpeed, 0, 100, 0, 255);
  
   if(motorName == 0)
  {
     switch (operation)
     {
        case 0:
          // brake
          digitalWrite(in1, LOW);
          digitalWrite(in2, LOW);
          break;
          
        case 1:
          // forward
          digitalWrite(in2, LOW);
          digitalWrite(in1, HIGH);
          analogWrite(enA, motorSpeed);
          break;
          
        case 2:
          // reverse
          digitalWrite(in1, LOW);
          digitalWrite(in2, HIGH);
          analogWrite(enA, motorSpeed);
          break;
          
        case 3:
          // off
          digitalWrite(in1, LOW);
          digitalWrite(in2, LOW);
          analogWrite(enA, LOW);
          break;
          
        default:
          break;
     }
  }
 else if(motorName == 1)
  {
    switch (operation)
    {
     case 0:
          // brake
          digitalWrite(in3, LOW);
          digitalWrite(in4, LOW);
          break;
          
        case 1:
          // forward
          digitalWrite(in4, LOW);
          digitalWrite(in3, HIGH);
          analogWrite(enB, motorSpeed);
          break;
          
        case 2:
          // reverse
          digitalWrite(in3, LOW);
          digitalWrite(in4, HIGH);
          analogWrite(enB, motorSpeed);
          break;
          
        case 3:
          // off
          digitalWrite(in3, LOW);
          digitalWrite(in4, LOW);
          analogWrite(enB, LOW);
          break;
          
         default:
          break;
     }
  } 
}

String getValue(String data, char separator, int index)
{

    int maxIndex = data.length()-1;
    int j=0;
    String chunkVal = "";

    for(int i=0; i<=maxIndex && j<=index; i++)
    {
      chunkVal.concat(data[i]);

      if(data[i]==separator)
      {
        j++;

        if(j>index)
        {
          chunkVal.trim();
          return chunkVal;
        }    

        chunkVal = "";    
      }  
    }  
}
