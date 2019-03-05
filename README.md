# gestual_recognition
Short project to identify planes of movement on given mocaps. Using unity and C# scripts for detection and visualisation of the movement planes.

Instruction for creating a scene:
1. Open Unity, open project GIT_Unity2018
2. File > New Scene
3. Save scene as Scene_Sample under Assets/Scenes/Scenes_Mines
4. Create new object with Right-Click in empty hierarchy > 3D object > Plane
5. Add NeuronRobot by sliding Assets/Neuron/Prefabs/NeuronRobot.prefab onto the
   scene
6. Add Mocap by clicking on NeuronRobot in the hierarchy and selecting
   Controller on the right panel. Browse for
   Assets/Neuron/NeuronExamples/SimpleFBXAnimations/ and Example Animator
   Controller.
7. Add Trail by unfolding in the hierarchy NeuronRobot > Robot_Reference >
   Robot_Hips > ... > Robot_LeftHand. On the right panel click add component and
   add a trail
8. Reduce width from 1.0 m to 4 cm for example
9. Add script for example
   Assets/Scripts/Scripts_VM/MotionView/MotionViewRibbon.cs by sliding the file
   on the Robot_LeftHand for example.
10. Create two new material anywhere in the project and add these into Line
    Materials and Ribbon Materials.
10. Download the package Characters.unitypackage from http://www.jfcad.com/Mines/. 
11. Import package using from the menu Assets > Import package > Custom package
    and choose the package you have just downloaded. This should a folder
    Standard Assets to your project.
12. **Add avatar controlled by the user** from Standard Assets/Characters/ThirdPersonCharacter/Prefabs/ ThirdPersonController.prefab

