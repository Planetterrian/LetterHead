using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AvatarManager : Singleton<AvatarManager>
{
    public Texture2D[] avatars;
    public Sprite[] avatars_sprites;

    public Texture2D GetAvatarImage(string avatarName)
    {
        return avatars.FirstOrDefault(a => a.name == avatarName);
    }

    private void Start()
    {
    }
}