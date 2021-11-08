#ifndef SCENE_LIGHT_H
#define SCENE_LIGHT_H

#include "framework.h"
#include "texture.h"

struct sSceneData {
	struct {
		Vector3 position = { 5.0f, 5.0f, 5.0f };
		Vector4 color = { 1.0f, 1.0f, 1.0f, 1.0f };
		Vector3 diffuse;
		Vector3 specular;
		Vector3 ambient;
	} light;
	
	Texture* enviorment_cubemap = NULL;
	Texture* enviorment_prem[5] = { NULL };

	sSceneData() {
		enviorment_cubemap = new Texture();

		for (int i = 0; i < 5; i++) {
			enviorment_prem[i] = new Texture();
		}
	}

	~sSceneData() {
		delete enviorment_cubemap;

		for (int i = 0; i < 5; i++) {
			delete enviorment_prem[i];
		}
	}
};
#endif