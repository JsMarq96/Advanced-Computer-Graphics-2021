#include "material.h"
#include "texture.h"
#include "application.h"
#include "extra/hdre.h"

StandardMaterial::StandardMaterial()
{
	color = vec4(1.f, 1.f, 1.f, 1.f);
	shader = Shader::Get("data/shaders/basic.vs", "data/shaders/flat.fs");
}

StandardMaterial::~StandardMaterial()
{

}

void StandardMaterial::setUniforms(Camera* camera, Matrix44 model)
{
	//upload node uniforms
	shader->setUniform("u_viewprojection", camera->viewprojection_matrix);
	shader->setUniform("u_camera_position", camera->eye);
	shader->setUniform("u_model", model);
	shader->setUniform("u_time", Application::instance->time);
	shader->setUniform("u_output", Application::instance->output);

	shader->setUniform("u_color", color);
	shader->setUniform("u_exposure", Application::instance->scene_exposure);

	if (texture)
		shader->setUniform("u_texture", texture);
}

void StandardMaterial::render(Mesh* mesh, Matrix44 model, Camera* camera)
{
	if (mesh && shader)
	{
		//enable shader
		shader->enable();

		//upload uniforms
		setUniforms(camera, model);

		//do the draw call
		mesh->render(GL_TRIANGLES);

		//disable shader
		shader->disable();
	}
}

void StandardMaterial::renderInMenu()
{
	ImGui::ColorEdit3("Color", (float*)&color); // Edit 3 floats representing a color
}

WireframeMaterial::WireframeMaterial()
{
	color = vec4(1.f, 1.f, 1.f, 1.f);
	shader = Shader::Get("data/shaders/basic.vs", "data/shaders/flat.fs");
}

WireframeMaterial::~WireframeMaterial()
{

}

void WireframeMaterial::render(Mesh* mesh, Matrix44 model, Camera * camera)
{
	if (shader && mesh)
	{
		glPolygonMode(GL_FRONT_AND_BACK, GL_LINE);

		//enable shader
		shader->enable();

		//upload material specific uniforms
		setUniforms(camera, model);

		//do the draw call
		mesh->render(GL_TRIANGLES);

		glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
	}
}


// =================
// CUSTOM MATERIALS
// =================

// TEXTURED MATERIAL
// Since the base class already has support for texturing, we just need to 
// change the shaders
TexturedMaterial::TexturedMaterial(const char* texture_name) {
	assert(texture_name != NULL && "Texture of Textured material cannot be null");

	color = vec4(1.f, 1.f, 1.f, 1.f);
	shader = Shader::Get("data/shaders/basic.vs", "data/shaders/textured.fs");
	texture = Texture::Get(texture_name);
}
TexturedMaterial::~TexturedMaterial() {

}


void TexturedMaterial::renderInMenu() {
	// TODO: Imgui integration
}


// PHONG ILUMNATION MATERIAL
PhongMaterial::PhongMaterial() {
	shader = Shader::Get("data/shaders/phong.vs", "data/shaders/phong.fs");
}
PhongMaterial::~PhongMaterial(){}

void PhongMaterial::setUniforms(Camera* camera, Matrix44 model) {
	StandardMaterial::setUniforms(camera, model);

	// Phong essential uniforms
	shader->setUniform("u_material_ambient", ambient_value);
	shader->setUniform("u_material_diffuse", diffuse_value);
	shader->setUniform("u_material_specular", specular_value);
	shader->setUniform("u_material_shininess", shiniess);

	shader->setUniform("u_light_ambient", scene_light.ambient);
	shader->setUniform("u_light_diffuse", scene_light.diffuse);
	shader->setUniform("u_light_specular", scene_light.specular);
	shader->setUniform("u_light_color", scene_light.color);
	shader->setUniform("u_light_position", scene_light.position);
}
void PhongMaterial::renderInMenu() {
	ImGui::Text("Material properties:");
	ImGui::ColorEdit3("M Color", (float*)&color);
	ImGui::SliderFloat("M Ambient", &ambient_value, 0.0f, 1.0f);
	ImGui::SliderFloat("M Diffuse", &diffuse_value, 0.0f, 1.0f);
	ImGui::SliderFloat("M Specular", &specular_value, 0.0f, 1.0f);
	ImGui::SliderFloat("M Shininess", &shiniess, 0.0f, 64.0f);	
}