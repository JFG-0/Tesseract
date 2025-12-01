#include <Wire.h>
#include <MPU6050.h>
#include <MadgwickAHRS.h>

MPU6050 mpu;
Madgwick filter;

const float sampleFreq = 100.0f; // Hz
const float faceThresholdDeg = 15.0f;
unsigned long lastPrint = 0;

void setup() {
  Serial.begin(115200);
  Wire.begin();

  mpu.initialize();
  if (!mpu.testConnection()) {
    Serial.println("MPU6050 connection failed");
    while (1);
  }
  filter.begin(sampleFreq);
}

int classifyFaceFromGravity(float gx, float gy, float gz) {
  float mag = sqrtf(gx*gx + gy*gy + gz*gz);
  if (mag < 1e-6f) return 0;
  gx /= mag; gy /= mag; gz /= mag;

  struct Face { int id; float nx, ny, nz; } faces[6] = {
    {1,  0,  0,  1}, // Top
    {6,  0,  0, -1}, // Bottom
    {2,  1,  0,  0}, // Right
    {3, -1,  0,  0}, // Left
    {4,  0,  1,  0}, // Front
    {5,  0, -1,  0}  // Back
  };

  float bestAngle = 1e9f; int bestId = 0;
  for (int i = 0; i < 6; i++) {
    float dotp = gx*faces[i].nx + gy*faces[i].ny + gz*faces[i].nz;
    dotp = constrain(dotp, -1.0f, 1.0f);
    float angleDeg = acosf(dotp) * 180.0f / PI;
    if (angleDeg < bestAngle) { bestAngle = angleDeg; bestId = faces[i].id; }
  }
  if (bestAngle <= faceThresholdDeg) return bestId;
  return 0; // Undefined
}

void loop() {
  int16_t ax, ay, az, gx_raw, gy_raw, gz_raw;
  mpu.getMotion6(&ax, &ay, &az, &gx_raw, &gy_raw, &gz_raw);

  float axf = ax / 16384.0f;
  float ayf = ay / 16384.0f;
  float azf = az / 16384.0f;

  float gxf = gx_raw / 131.0f;
  float gyf = gy_raw / 131.0f;
  float gzf = gz_raw / 131.0f;

  filter.updateIMU(gxf * DEG_TO_RAD, gyf * DEG_TO_RAD, gzf * DEG_TO_RAD,
                   axf, ayf, azf);

  int face = classifyFaceFromGravity(axf, ayf, azf);

  if (millis() - lastPrint >= 500) {
    Serial.println(face); // print only integer (0–6)
    lastPrint = millis();
  }

  delay(10); // ~100 Hz loop
}