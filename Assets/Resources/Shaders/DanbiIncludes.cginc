// 1. Resources.
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles

static const float PI = 3.14159265f;
static const float EPS = 1e-8;

float4x4 _CameraToWorldMat;
float3 CameraPosInWorld; /*Internal*/
float3 CameraViewDirection; /*Internal*/
float4x4 _Projection;
float4x4 _CameraInverseProjection;
float2 _PixelOffset;

Texture2D<float4> _PanoramaImage;
SamplerState sampler_PanoramaImage;

int _MaxBounce;
RWTexture2D<float4> _DistortedImage;

struct Ray {
  float3 origin;
  float3 direction;
  float3 localDirectionInCamera;
  float3 energy;
};

struct RayHit {
  float3 position; // hit position on the surface.
  float2 uvInTriangle; // relative barycentric coords of the hit position.
  float3x2 vertexUVs;
  float distance;
  float3 normal; // normal at the ray hit position.
  float3 specular;
};

/*
* All geometries
*/
StructuredBuffer<float3> _Vertices;
StructuredBuffer<int> _Indices;
StructuredBuffer<float2> _Texcoords;

/*
* Camera External Parameters
*/
float _FOV_Rad;

int _IterativeCounter;
int _IterativeSafeCounter;
float4 _IterativeThreshold;

struct CameraExternalData {
  float3 RadialCoefficient;
  float3 TangentialCoefficient;
  float2 PrincipalPoint;
  float2 FocalLength;
  float SkewCoefficient; // Rarely used recently since cameras has been becoming better.
};

StructuredBuffer<CameraExternalData> _CameraExternalData;

Ray CreateRay(float3 origin, float3 direction, float3 localDirectionInCamera);
Ray CreateCameraRay(float2 undistortedNDC);
RayHit CreateRayHit();
float3 Shade(inout Ray ray, RayHit resHit);
bool IntersectTriangle_MT97(Ray ray, float3 vtx0, float3 vtx1, float3 vtx2, out float t, out float u, out float v);

float2 normalize(float x_u, float y_u, in CameraExternalData data);
float2 denormalize(float x_u, float y_u, in CameraExternalData data);
float2 distort_normalized(float x_nu, float y_nu, in CameraExternalData data);
float2 undistortNDC_newton(float2 p_d, in CameraExternalData data);
float2 undistortNDC_iterative(float2 p_d, in CameraExternalData data);
float2 undistortNDC_direct(float2 p_d, in CameraExternalData data);


// Ray------------------------------------------------------------------------------------------

Ray CreateRay(float3 origin, float3 direction, float3 localDirectionInCamera) {
  Ray res = (Ray)0;
  res.origin = origin;
  res.direction = direction;
  res.localDirectionInCamera = localDirectionInCamera;
  res.energy = float3(1.0f, 1.0f, 1.0f);
  return res;
}

Ray CreateCameraRay(float2 undistortedNDC) {
  uint width = 0, height = 0;
  _DistortedImage.GetDimensions(width, height);

  // Transform the camera origin onto the world space.
  float3 cameraCurPixelInWorld = mul(_CameraToWorldMat, float4(0.0f, 0.0f, 0.0f, 1.0f)).xyz;
  // Invert the perspective projection of the view-space position.
  float3 posInCameraZero = mul(_CameraInverseProjection, float4(undistortedNDC, 0.0f, 1.0f)).xyz;
  float3 localDirectionInCamera = normalize(posInCameraZero);
  // Transform the direction from camera to world space and normalize.
  float3 dirInWorld = mul(_CameraToWorldMat, float4(posInCameraZero, 0.0f)).xyz;
  dirInWorld = normalize(dirInWorld);

  return CreateRay(cameraCurPixelInWorld, dirInWorld, localDirectionInCamera);
}

RayHit CreateRayHit() {
  RayHit res = (RayHit)0;
  res.position = (float3)0;
  res.vertexUVs = (float3x2)0;
  res.distance = 1.#INF;
  res.normal = (float3)0;
  res.uvInTriangle = (float2)0;  
  res.specular = (float3)0;

  return res;
}

// Ray------------------------------------------------------------------------------------------

float3 Shade(inout Ray ray, RayHit resHit) {
  uint width = 0, height = 0;
  _DistortedImage.GetDimensions(width, height);
  
  ray.origin = resHit.position + resHit.normal * 0.001;
  ray.direction = reflect(ray.direction, resHit.normal);
  ray.energy *= resHit.specular;
  
  float2 uv = resHit.uvInTriangle;
  float2 uvTex = (1 - uv[0] - uv[1]) * resHit.vertexUVs[0]
               + uv[0] * resHit.vertexUVs[1]
               + uv[1] * resHit.vertexUVs[2];
  return _PanoramaImage.SampleLevel(sampler_PanoramaImage, uvTex, 0).xyz;
}

bool IntersectTriangle_MT97(Ray ray, float3 vtx0, float3 vtx1, float3 vtx2, out float t, out float u, out float v) {
  t = 1.#INF;
  // find vectors for two edges sharing vertices.
  float3 edge1 = vtx1 - vtx0;
  float3 edge2 = vtx2 - vtx0;

  // begin calculating determinant - it's also used to calculate U param.
  float3 pvec = cross(ray.direction, edge2);

  // if determinant is near zero, ray lies in plane of triangle.
  float det = dot(edge1, pvec);

  // use backface culling.
  if (abs(det) < EPS) {
    return false;
  }

  float inv_det = 1.0 / det;

  // calculate distance from vertex0 to ray.origin.
  float3 tvec = ray.origin - vtx0;

  // calculate U param and test bounds.
  u = dot(tvec, pvec) * inv_det;

  if (u < 0.0 || u > 1.0) {
    v = 1.#INF;
    return false;
  }

  float3 qvec = cross(tvec, edge1);
  
  // prepare to test V param.
  v = dot(ray.direction, qvec) * inv_det;

  if (v < 0.0 || u + v > 1.0) {
    return false;
  }
    
  // calculate t param, ray intersects on the triangle.
  t = dot(edge2, qvec) * inv_det;
  
  return true;
}

float2 normalize(float x_u, float y_u, in CameraExternalData data) {
  float fx = data.FocalLength.x;
  float fy = data.FocalLength.y;
  float cx = data.PrincipalPoint.x;
  float cy = data.PrincipalPoint.y;

  // return float2(x_n, y_n).
  return float2((y_u - cy) / fy, (x_u - cx) / fx);
}

float2 denormalize(float x_u, float y_u, in CameraExternalData data) {
  float fx = data.FocalLength.x;
  float fy = data.FocalLength.y;

  float cx = data.PrincipalPoint.x;
  float cy = data.PrincipalPoint.y;

  float x_p = fx * x_u + cx;
  float y_p = fy * y_u + cy;
  return float2(x_p, y_p);
}

float2 distort_normalized(float x_nu, float y_nu, in CameraExternalData data) {
  float k1 = data.RadialCoefficient.x;
  float k2 = data.RadialCoefficient.y;
  // k3 is in no use.

  float p1 = data.TangentialCoefficient.x;
  float p2 = data.TangentialCoefficient.y;

  float r2 = x_nu * x_nu + y_nu * y_nu;

  float radial_d = 1.0f +
                   (k1 * r2) +
                   (k2 * r2 * r2);

  float x_nd = (radial_d * x_nu) + 
               (2 * p1 * x_nu * y_nu) +
               (p2 * (r2 + 2 * x_nu * y_nu));

  float y_nd = (radial_d * y_nu) +
               (p1 * (r2 + 2 * y_nu * y_nu)) +
               (2 * p2 * x_nu * y_nu);
  return float2(x_nd, y_nd);
}

float2 undistortNDC_newton(float2 p_d, in CameraExternalData data) {
  /*int i = 0;
    while (i < N) {
      i += 1;
      d = 1 + k1 * (s * s + t * t) + k2 * (s * s * s * s) + (2 * s * s * t * t) + (t * t * t * t);

      f1 = -u + (s * d + (2 * p1 * s * t + p2 * (s * s + t * t + 2 * s * s))) * fx * cx;
      f2 = -v + (t * d + (p1 * (s * s + t * t + 2 * t * t) + 2 * p2 * s * t)) * fy + cy;
      j1s = fx * (1 + k1 * (3 * s * s + t * t) + k2 * ((5 * s * s + 6 * t * t) * s * s + t * t * t * t)) + 2 * p1 * fx * t + 6 * p2 * fx * s;
      j1t = fx * (2 * k1 * s * t + 4 * k2 * (s * s * s * t + s * t * t * t)) + 2 * p1 * fx * s + 2 * p2 * fx * t;
      j2s = fy * (2 * k1 * s * t + 4 * k2 * (s * s * s * t + s * t * t * t)) + 2 * p1 * fy * s + 2 * p2 * fy * t;
      j2t = fy * (1 + k1 * (s * s + 3 * t * t) + 3 * t * t) + k2 * (s * s * s * s + (6 * s * s + 5 * t * t) * t * t)) + 6 * p1 * fy * t + 2 * p2 * fy * s;

      d = (j1s * j2t - j1t * j2s);

      S = s - (j2t * f1 - j1t * f2) / d;
      T = t - (j2s * f1 - j1s * f2) / d;

      if (abs(S - s) < err_threshold && abs(T - t) < err_threshold) {
        break;
      }

      s = S;
      t = T;
    }*/
  return (float2)0;
}

float2 undistortNDC_iterative(float2 p_d, in CameraExternalData data) {
  float2 p_nuInitialGuess = normalize(p_d.x, p_d.y, data);
  float2 p_nu = p_nuInitialGuess;

  while (true) {
    float2 err = distort_normalized(p_nu.x, p_nu.y, data);
    err -= p_nuInitialGuess;
    p_nu -= err;

    ++_IterativeCounter;
    if (_IterativeCounter >= _IterativeSafeCounter) {
      _IterativeCounter = 0;
      break;
    }

    if (err.x < _IterativeThreshold.x && err.y < _IterativeThreshold.y) {
      break;
    }
  }

  float2 p_nu_denormalized = denormalize(p_nu.x, p_nu.y, data);
  return p_nu_denormalized;
}

float2 undistortNDC_direct(float2 p_d, in CameraExternalData data) {
  // [p_d.x, p_d.y] = [fx_0, fy_0] * [xn_d, yn_d] + [cx, cy].
  float xn_d = (p_d.x - data.PrincipalPoint.x) / data.FocalLength.x;
  float yn_d = (p_d.y - data.PrincipalPoint.y) / data.FocalLength.y;
  // distance = r ^ 2.
  float rsqr = xn_d * xn_d + yn_d * yn_d;
  // r ^ 4
  float rqd = rsqr * rsqr;

  float k1 = data.RadialCoefficient.x;
  float k2 = data.RadialCoefficient.y;
  float p1 = data.TangentialCoefficient.x;
  float p2 = data.TangentialCoefficient.y;
  
  float d1 = k1 * rsqr + k2 * rqd;
  float d2 = (4 * k1 * rsqr) + (6 * k2 * rqd) + (8 * p1 * xn_d) + (8 * p2 * yn_d + 1);
  d2 = 1 / d2;

  float xn_u = xn_u - d2 * (d1 * xn_d) +
               (2 * p1 * xn_d * yn_d) +
               (8 * p1 * xn_d) +
               (8 * p2 * yn_d + 1);
  float yn_u = yn_u - d2 * (d1 * yn_d) +
               p1 * (rsqr + 2 * yn_d * yn_d) +
               (2 * p2 * xn_d * yn_d);
  float x_u = xn_u * data.FocalLength.x + data.PrincipalPoint.x;
  float y_u = yn_u * data.FocalLength.y + data.PrincipalPoint.y;
  return float2(x_u, y_u);
}