#include "volumetric_node.h"

VolumetricNode::VolumetricNode() {
	material = new VolumetricMaterial();
	((VolumetricMaterial*) material)->setVolume(VOLUMES_DIR[volume_id]);

	mesh = new Mesh();
	mesh->createCube();
}


void VolumetricNode::renderInMenu() {

}