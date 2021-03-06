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
	ImGui::ColorEdit3("Base color", (float*)&color);
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

	shader->setUniform("u_light_ambient", scene_data.light.ambient);
	shader->setUniform("u_light_diffuse", scene_data.light.diffuse);
	shader->setUniform("u_light_specular", scene_data.light.specular);
	shader->setUniform("u_light_color", scene_data.light.color);
	shader->setUniform("u_light_position", scene_data.light.position);
}
void PhongMaterial::renderInMenu() {
	ImGui::Text("Material properties:");
	ImGui::ColorEdit3("Color", (float*)&color);
	ImGui::SliderFloat("Ambient", &ambient_value, 0.0f, 1.0f);
	ImGui::SliderFloat("Diffuse", &diffuse_value, 0.0f, 1.0f);
	ImGui::SliderFloat("Specular", &specular_value, 0.0f, 1.0f);
	ImGui::SliderFloat("Shininess", &shiniess, 0.0f, 64.0f);	
}


// SKYBOX MATERIAL
SkyBoxMaterial::SkyBoxMaterial() {
	texture = new Texture();

	shader = Shader::Get("data/shaders/basic.vs", "data/shaders/skybox.fs");
}
SkyBoxMaterial::~SkyBoxMaterial() {
	delete texture;
}

void SkyBoxMaterial::setCubemapTexture(const char* cubemap_dir) {
	// Note, the clear does not free the memmory of cubemaps causing a memmory leak
	texture->clear();
	texture->cubemapFromImages(cubemap_dir);
	scene_data.enviorment_cubemap = texture;
}

void SkyBoxMaterial::renderInMenu() {}


// REFLECTIVE MATERIAL
ReflectiveMaterial::ReflectiveMaterial() {
	shader = Shader::Get("data/shaders/basic.vs", "data/shaders/reflective.fs");
}

void ReflectiveMaterial::setUniforms(Camera* camera, Matrix44 model) {
	// Set the current texture as the shared cubemap
	texture = scene_data.enviorment_cubemap;

	shader->setUniform("u_reflectiveness", reflectiveness);
	
	StandardMaterial::setUniforms(camera, model);
}

void ReflectiveMaterial::renderInMenu() {
	ImGui::SliderFloat("Reflectiveness", &reflectiveness, 0.0f, 1.0f);
}

// HDRe MATERIAL
HDReMaterial::HDReMaterial() {
	for (int i = 0; i < 5; i++) {
		prem[i] = new Texture();
	}

	enviorment = new Texture();

	shader = Shader::Get("data/shaders/basic.vs", "data/shaders/skybox.fs");
}

HDReMaterial::~HDReMaterial() {
	for (int i = 0; i < 5; i++) {
		delete prem[i];
	}

	delete enviorment;
}


void HDReMaterial::setUniforms(Camera* camera, Matrix44 model) {

	if (display_level == 0) {
		shader->setTexture("u_texture", scene_data.enviorment_cubemap);
	} else {
		shader->setTexture("u_texture", scene_data.enviorment_prem[display_level-1]);
	}

	shader->setUniform("u_tonemap_mode", (float)Application::instance->tonemap);
	StandardMaterial::setUniforms(camera, model);
}


// PBR MATERIAL
PBRMaterial::PBRMaterial(const char* albedo_dir,
		const char* roughness_dir,
		const char* metalness_dir,
		const char* normal_dir,
		const char* opacity_dir) {
	albedo_map = Texture::Get(albedo_dir);
	roughness_map = Texture::Get(roughness_dir);
	metalness_map = Texture::Get(metalness_dir);
	opacity_map = Texture::Get(opacity_dir);
	normal_map = Texture::Get(normal_dir);
	brdf_LUT = Texture::Get("data/brdfLUT.png");

	shader = Shader::Get("data/shaders/IBL.vs", "data/shaders/IBL.fs");

	texture_mode = SEPARETE_TEXTURES;
}

PBRMaterial::PBRMaterial(const char*         albedo_dir,
						 const char*         roughness_dir, 
						 const char*         metalness_dir,
						 const char*		 normal_dir,
						 const ePBR_Format   itexture_mode) {
	albedo_map = Texture::Get(albedo_dir);
	roughness_map = Texture::Get(roughness_dir);
	metalness_map = Texture::Get(metalness_dir);
	normal_map = Texture::Get(normal_dir);
	brdf_LUT = Texture::Get("data/brdfLUT.png");


	shader = Shader::Get("data/shaders/IBL.vs", "data/shaders/IBL.fs");

	texture_mode = itexture_mode;
}

PBRMaterial::PBRMaterial(const char* albedo_dir,
						 const char* roughness_dir, 
						 const char* normal_dir) {
	albedo_map = Texture::Get(albedo_dir);
	roughness_map = Texture::Get(roughness_dir);
	normal_map = Texture::Get(normal_dir);
	brdf_LUT = Texture::Get("data/brdfLUT.png");


	shader = Shader::Get("data/shaders/IBL.vs", "data/shaders/IBL.fs");

	texture_mode = MINECRAFT_PBR;
}

PBRMaterial::~PBRMaterial() {
	delete opacity_map;
	delete albedo_map;
	delete roughness_map;
	delete normal_map;
	delete metalness_map;
	delete brdf_LUT;
}

void PBRMaterial::setUniforms(Camera* camera, Matrix44 model) {
	// Upload all the textures for the IBL calculations#
	shader->setTexture("u_texture_enviorment", scene_data.enviorment_cubemap);
	shader->setTexture("u_texture_prem_0", scene_data.enviorment_prem[0]);
	shader->setTexture("u_texture_prem_1", scene_data.enviorment_prem[1]);
	shader->setTexture("u_texture_prem_2", scene_data.enviorment_prem[2]);
	shader->setTexture("u_texture_prem_3", scene_data.enviorment_prem[3]);
	shader->setTexture("u_texture_prem_4", scene_data.enviorment_prem[4]);

	// Light info
	shader->setUniform("u_light_radiance", scene_data.light.color);
	shader->setUniform("u_light_position", scene_data.light.position);

	// Upload PBR texteure maps
	if (albedo_map) shader->setTexture("u_albedo_map", albedo_map);
	if (roughness_map) shader->setTexture("u_rough_map", roughness_map);
	if (metalness_map) shader->setTexture("u_metal_map", metalness_map);
	if (normal_map) shader->setTexture("u_normal_map", normal_map);
	shader->setTexture("u_brdf_LUT", brdf_LUT);

	// Material properties
	shader->setUniform("u_metalness_mult", metalness_mult);
	shader->setUniform("u_roughness_mult", roughness_mult);

	shader->setUniform("u_output_mode", (float) Application::instance->output);
	shader->setUniform("u_tonemap_mode", (float)Application::instance->tonemap);
	shader->setUniform("u_material_mode", (float)(int)texture_mode);

	// POM parameters
	shader->setUniform("u_POM_enable", (enable_POM) ? 1.0f : 0.0f);
	shader->setUniform("u_POM_resolution", (float) POM_resulution);
	shader->setUniform("u_POM_depth", POM_depth);

	// Opacity map
	shader->setUniform("u_opacity_enable", (enable_opacity) ? 1.0f : 0.0f);
	if (enable_opacity && opacity_map) {
		shader->setTexture("u_opacity_map", opacity_map);
	}
	
	StandardMaterial::setUniforms(camera, model);
}

void PBRMaterial::render(Mesh* mesh, Matrix44 model, Camera* camera) {

	if (opacity_map) {
		glEnable(GL_CULL_FACE);
		glCullFace(GL_BACK);
		glEnable(GL_BLEND);
		glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
	}

	StandardMaterial::render(mesh, model, camera);
	
	GLenum err_code;
	if ((err_code = glGetError()) != GL_NO_ERROR) {
		std::cout << (err_code) << " " << glewGetErrorString(err_code) << std::endl;
	}

	if (opacity_map) {
		glDisable(GL_CULL_FACE);
		glDisable(GL_BLEND);
	}
}

void PBRMaterial::renderInMenu() {
	ImGui::SliderFloat("Metalness mult.", &metalness_mult, 0.005f, 2.0f);
	ImGui::SliderFloat("Roughness mult.", &roughness_mult, 0.005f, 2.0f);

	ImGui::Checkbox("Enable Opacity", &enable_opacity);
	ImGui::Checkbox("Enable POM", &enable_POM);
	
	if (enable_POM) {
		ImGui::SliderInt("POM resolution", &POM_resulution, 1, 200);
		ImGui::SliderFloat("POM depth", &POM_depth, 0.005f, 1.0f);
	}
}