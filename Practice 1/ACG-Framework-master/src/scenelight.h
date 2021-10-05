#ifndef SCENE_LIGHT_H
#define SCENE_LIGHT_H

#include "framework.h"

struct sSceneLight {
	Vector3 position = { 5.0f, 5.0f, 5.0f };
	Vector4 color = { 1.0f, 1.0f, 1.0f, 1.0f };
	float diffuse;// = { 1.0f, 1.0f, 1.0f };
	float specular;// = { 1.0f, 1.0f, 1.0f };
	float ambient;// = { 0.25f, 0.25f, 0.25f };
};
#endif