#define PULSE        11
#define DIR    12
#define EN           13

#define B_ENABLE 1
#define B_DISABLE 0
#define B_CW 2
#define B_CCW 3
#define B_CRD  4

void rotate();
void rotate_count(int cnt);
 
int pul = 120;
int freq_spd=0;
int en=0;


void setup(){
  pinMode(PULSE, OUTPUT);
  pinMode(DIR, OUTPUT);  
  pinMode(EN, OUTPUT); 
  Serial.begin(9600);
}

void loop(){
  if(Serial.available()){
    int ch = Serial.parseInt();
    int ch2 = 0;
    int ch3 = 0;
    Serial.print("Input\n");
    switch(ch){
      // disable
      case B_DISABLE: 
      en=0;
      break;
      // enable
      case B_ENABLE:
      digitalWrite(EN, HIGH);
      en=1;
      break;
      // clockwise
      case B_CW: 
      digitalWrite(DIR, HIGH);
      break;
      // counterclockwise
      case B_CCW: 
      digitalWrite(DIR, LOW);
      break;
      case B_CRD:
      ch2 = Serial.parseInt();
      ch3 = Serial.parseInt();
      if(ch2 < 0) {
        ch2 = 0;
      }
      rotate_count(ch2, ch3);
      ch2 = 0;
      break;
      default:
      if(ch > 99 && ch < 251) {
        pul = ch;
      }
      break;
    }
    while(Serial.available() > 0) {
      Serial.read();
    }
  }
  if(en) {
    Serial.print("Rotate\n");
    rotate();
  } else {
    delay(100);
  }
}

void rotate_count(int dir, int cnt) {
  int i=0;
  if(dir == B_CW) {
    digitalWrite(DIR, HIGH);
    Serial.print("CW\n");
  } else if(dir == B_CCW) {
    digitalWrite(DIR, LOW);
    Serial.print("CCW\n");
  }
  digitalWrite(EN, HIGH);
  for(i=0; i<cnt; i++) {
    freq_spd = (256-pul)*100;
    if(freq_spd<400) {
      freq_spd = 400;
    }
    digitalWrite(PULSE, HIGH);
    delayMicroseconds(freq_spd);
    digitalWrite(PULSE, LOW);
    delayMicroseconds(freq_spd);
  }
  en = 0;
}

void rotate() {
  if(pul==0) {
    digitalWrite(PULSE, LOW);
  } else {
    freq_spd = (256-pul)*100;
    if(freq_spd<400) {
      freq_spd = 400;
    }
    digitalWrite(PULSE, HIGH);
    delayMicroseconds(freq_spd);
    digitalWrite(PULSE, LOW);
    delayMicroseconds(freq_spd);
  }
}
