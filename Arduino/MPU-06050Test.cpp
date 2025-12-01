#include <Wire.h>

#define MPU6050_ADDR 0x68

void setup() {
  Serial.begin(115200);
  while (!Serial);
  Wire.begin();

  // Wake up MPU6050 (clear sleep bit)
  Wire.beginTransmission(MPU6050_ADDR);
  Wire.write(0x6B); // Power management register
  Wire.write(0x00); // Set to zero (wakes up the MPU6050)
  Wire.endTransmission();

  Serial.println("MPU6050 init done");
}

void loop() {
  int16_t ax, ay, az;
  int16_t gx, gy, gz;

  // Read accelerometer (6 bytes starting at 0x3B)
  Wire.beginTransmission(MPU6050_ADDR);
  Wire.write(0x3B);
  Wire.endTransmission(false);
  Wire.requestFrom(MPU6050_ADDR, 6);
  ax = Wire.read() << 8 | Wire.read();
  ay = Wire.read() << 8 | Wire.read();
  az = Wire.read() << 8 | Wire.read();

  // Read gyroscope (6 bytes starting at 0x43)
  Wire.beginTransmission(MPU6050_ADDR);
  Wire.write(0x43);
  Wire.endTransmission(false);
  Wire.requestFrom(MPU6050_ADDR, 6);
  gx = Wire.read() << 8 | Wire.read();
  gy = Wire.read() << 8 | Wire.read();
  gz = Wire.read() << 8 | Wire.read();

  Serial.print("Accel: ");
  Serial.print(ax); Serial.print(", ");
  Serial.print(ay); Serial.print(", ");
  Serial.print(az);

  Serial.print(" | Gyro: ");
  Serial.print(gx); Serial.print(", ");
  Serial.print(gy); Serial.print(", ");
  Serial.println(gz);

  delay(200);
}