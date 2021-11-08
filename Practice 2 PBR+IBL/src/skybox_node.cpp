#include "skybox_node.h"


SkyboxNode::SkyboxNode() {
	material = new SkyBoxMaterial();
	mesh = new Mesh();

	mesh->createCube();

	((SkyBoxMaterial*)material)->setCubemapTexture(SKYBOXES[0]);

	this->name = "Skybox";
}
SkyboxNode::~SkyboxNode() {
	delete material;
	delete mesh;
}

void SkyboxNode::render(Camera* camera) {
	// Move the box of the skybox to the current camera position
	model.setTranslation(camera->eye.x, camera->eye.y, camera->eye.z);

	if (material) {
		glDisable(GL_DEPTH_TEST);
		material->render(mesh, model, camera);
		glEnable(GL_DEPTH_TEST);
	}
}

void SkyboxNode::renderInMenu() {
	bool has_changed = false;
	has_changed |= ImGui::Combo("Select", &enviorment_id, "City\0Dragonvale\0Snow\0");

	if (has_changed) {
		((SkyBoxMaterial*)material)->setCubemapTexture(SKYBOXES[enviorment_id]);
	}
}


// HDReSkybox methods ===============

HDReSkyboxNode::HDReSkyboxNode() {
	material = new HDReMaterial();
	mesh = new Mesh();

	mesh->createCube();

	set_HDRe(HDREs[0]);

	this->name = "HDRe Skybox";
}
HDReSkyboxNode::~HDReSkyboxNode() {
	delete material;
	delete mesh;
}

void HDReSkyboxNode::set_HDRe(const char* dir) {
	HDRE *curr_hdre = HDRE::Get(dir);

	scene_data.enviorment_cubemap->cubemapFromHDRE(curr_hdre, 0);

	scene_data.enviorment_prem[0]->cubemapFromHDRE(curr_hdre, 1);
	scene_data.enviorment_prem[1]->cubemapFromHDRE(curr_hdre, 2);
	scene_data.enviorment_prem[2]->cubemapFromHDRE(curr_hdre, 3);
	scene_data.enviorment_prem[3]->cubemapFromHDRE(curr_hdre, 4);
	scene_data.enviorment_prem[4]->cubemapFromHDRE(curr_hdre, 5);
}

void HDReSkyboxNode::render(Camera* camera) {
	// Move the box of the skybox to the current camera position
	model.setTranslation(camera->eye.x, camera->eye.y, camera->eye.z);

	if (material) {
		glDisable(GL_DEPTH_TEST);
		material->render(mesh, model, camera);
		glEnable(GL_DEPTH_TEST);
	}
}

void HDReSkyboxNode::renderInMenu() {
	bool has_changed = false;
	has_changed |= ImGui::Combo("Select", &hdre_id, "Panorama\0Pisa\0Studio\0San Giuseppe Brigde\0TV studio\0");

	if (has_changed) {
		set_HDRe(HDREs[hdre_id]);
	}

	ImGui::Text("Visualization only:");
	ImGui::SliderInt("Blur level", &((HDReMaterial*)material)->display_level, 0, 5);
}