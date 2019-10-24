# AR Car Configurator

**A complete Unity project designed for AR Car Configurators.**

**This project contains:**

## Light Estimation

Light estimation script provided in this project makes the model adept to the light in the physical environment.
The model will blend in with it's phyisical surrounding providing a more realistic look to the model.

## Models

In the demo version only 2 models can be used(BMW i8 & Audi A4 Allroad).
Both models can be placed, rotated and scaled in the physical environment.
There are 5 different carpaint colors available.
This project will contain a full list of cars from different brands. These will be used after the demo is finished.

## Rims

In the demo version only 4 rims are available.
All rims can be placed in the physical environment.
There are 3 different rim colors available.
This project will contain a full list of rims from different brands. These will be used after the demo is finished.

## Shadows

Both the models and rims provide shadows on the ARPlane.
These Shadows are made with a custom shader that is included in this project.

## Scripts

Scripts in this project will be constantly updated. The ARDemo script provides for everything needed in the Demo version(except the light estimation/).
All rims script provides all the rims that can be used.
All models script provides all the car models that can be used.
The UI script makes most of the user interfaces pop up when you click on an image in the toolbar.
The test script is only used for testing purposes before launching it in the Demo/Complete version.
ARTapToPlaceObject script is the first version of the AR Car Configurator script. this one is deprecated since the ARDemo script provides better quality.

## UI

**In the Demo version:**
The user interface in this project provides a loading screen before being able to place objects. this prevents the application from starting with errors.
Menu to choose between different car brands & car models.
Menu to choose between the rims. All the rims are provided with an image.
Color picker for both models and rims.
Delete option to remove the model and replace it.
2 buttons to manually rotate the model.
Automatic rotate button that rotates the model until you stop it.
Slider to set the size of the model
2 animated hints that show you how to search for ARPlanes and how to place the model.
ARPlane indicator. This shows where the model is going to be placed.
ARPlanes. These will generate planes where you could place the model.

All these will also be provided in the complete version unless the Demo version gets feedback to remove/change it.

## Builds

In the build folder there are .apk's of all previous/current versions of the project.
For now only Android builds have been made. iOS still needs to be tested and will be available in the near future.