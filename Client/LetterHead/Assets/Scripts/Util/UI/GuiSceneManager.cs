using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GuiSceneManager : MonoBehaviour
{
    public GuiScene[] guiScenes;
    public GuiScene startingGuiScene;

    private GuiScene nextGuiScene;
    private GuiScene lastGuiScene;

    private GuiScene currentGuiScene;
    public GuiScene CurrentGuiScene
    {
        get { return currentGuiScene; }
    }

    protected virtual void Start()
    {
        InitilizeGuiScenes();
        
        if(startingGuiScene)
            SetGuiScene(startingGuiScene);
    }


    private void InitilizeGuiScenes()
    {
        foreach (var gameGuiScene in guiScenes)
        {
            var scene = gameGuiScene;

            gameGuiScene.FadeOut(true);
            gameGuiScene.canvasFade.OnFadeOutCompleted.AddListener(() => OnGuiSceneHidden(scene));
        }
    }

    private void OnGuiSceneHidden(GuiScene gameGuiScene)
    {
        if (gameGuiScene == CurrentGuiScene && nextGuiScene != null)
        {
            nextGuiScene.FadeIn();
            currentGuiScene = nextGuiScene;
            nextGuiScene = null;
        }
    }


    public void SetGuiScene(string sceneName)
    {
        var scene = guiScenes.FirstOrDefault(s => s.gameObject.name == sceneName);
        if (!scene)
        {
            Debug.LogError("Invalid scene name: " + sceneName);
            return;
        }

        SetGuiScene(scene);
    }

    public void SetGuiScene(GuiScene scene)
    {
        lastGuiScene = CurrentGuiScene;

        if (CurrentGuiScene != null)
        {
            nextGuiScene = scene;
            CurrentGuiScene.FadeOut();
        }
        else
        {
            currentGuiScene = scene;
            scene.FadeIn();
        }

        scene.OnBeginShow();
    }
}