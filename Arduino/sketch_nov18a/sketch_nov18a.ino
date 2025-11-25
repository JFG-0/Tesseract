void setup() {
  Serial.begin(9600);
  pinMode(13, OUTPUT); // LED
  pinMode(2, INPUT_PULLUP); // Button on pin 2
}

void loop() {
  if (Serial.available()) {
    char command = Serial.read();
    if (command == '1') {
      digitalWrite(13, HIGH);
      Serial.println("LED ON");
    } else if (command == '0') {
      digitalWrite(13, LOW);
      Serial.println("LED OFF");
    }
  }

  // Check button state
  if (digitalRead(2) == LOW) { // Button pressed
    Serial.println("2"); // Send "2" to Unity
    delay(300); // Debounce delay
  }
}
