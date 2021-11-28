#include "volumetric_node.h"

VolumetricNode::VolumetricNode() {
	material = new VolumetricMaterial();
	((VolumetricMaterial*) material)->setVolume(VOLUMES_DIR[volume_id]);

	mesh = new Mesh();
	mesh->createCube();

	name = "Volumetric Node";
}


void VolumetricNode::renderInMenu()
{
	//Model edit
	if (ImGui::TreeNode("Model"))
	{
		float matrixTranslation[3], matrixRotation[3], matrixScale[3];
		ImGuizmo::DecomposeMatrixToComponents(model.m, matrixTranslation, matrixRotation, matrixScale);
		ImGui::DragFloat3("Position", matrixTranslation, 0.1f);
		ImGui::DragFloat3("Rotation", matrixRotation, 0.1f);
		ImGui::DragFloat3("Scale", matrixScale, 0.1f);
		ImGuizmo::RecomposeMatrixFromComponents(matrixTranslation, matrixRotation, matrixScale, model.m);

		ImGui::TreePop();
	}

	//Material
	if (material && ImGui::TreeNode("Material"))
	{
		material->renderInMenu();
		ImGui::TreePop();
	}

	//Geometry
	if (mesh && ImGui::TreeNode("Geometry"))
	{
		bool changed = false;
		changed |= ImGui::Combo("Volume", (int*)&volume_id, "BONSAI\0TEAPOT\0FEET\0");

		if (changed) {
			((VolumetricMaterial*)material)->setVolume(VOLUMES_DIR[volume_id]);
		}

		ImGui::TreePop();
	}
}