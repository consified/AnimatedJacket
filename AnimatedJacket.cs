using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public static class AnimatedJacket
{
    const int RenderSize = 1024;

    class State
    {
        public bool IsCenter;
        public int MusicId;
        public Texture StaticTexture;
        public VideoPlayer Player;
        public RenderTexture Target;
        public string CurrentPath;
    }

    static readonly Dictionary<int, State> States = new Dictionary<int, State>();
    static readonly Dictionary<int, int> TextureIds = new Dictionary<int, int>();
    static readonly Regex JacketId = new Regex(@"ui_jacket_(\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    static string[] SearchDirectories;
    static int PendingId;

    public static void Note(string assetPath)
    {
        int id;
        if (TryParseId(assetPath, out id))
            PendingId = id;
    }

    public static void Remember(Texture texture, string assetPath)
    {
        int id;
        if (texture == null || !TryParseId(assetPath, out id))
            return;
        TextureIds[texture.GetInstanceID()] = id;
        PendingId = id;
    }

    public static void OnJacketSet(UnityEngine.Object card, RawImage image)
    {
        try
        {
            if (card == null || image == null)
                return;
            State state = GetState(card.GetInstanceID());
            if (state.Target == null || image.texture != state.Target)
                state.StaticTexture = image.texture;
            state.MusicId = ResolveId(image.texture);
            Apply(image, state);
        }
        catch (Exception)
        {
        }
    }

    public static void OnCenterChanged(UnityEngine.Object card, RawImage image, bool isCenter)
    {
        try
        {
            if (card == null || image == null)
                return;
            State state = GetState(card.GetInstanceID());
            state.IsCenter = isCenter;
            if (state.StaticTexture == null || state.Target == null || image.texture != state.Target)
                state.StaticTexture = image.texture;
            if (state.MusicId <= 0)
                state.MusicId = ResolveId(image.texture);
            Apply(image, state);
        }
        catch (Exception)
        {
        }
    }

    public static void OnInfo(RawImage image, int musicId)
    {
        try
        {
            if (image == null)
                return;
            State state = GetState(image.GetInstanceID());
            state.IsCenter = true;
            state.MusicId = musicId;
            if (state.Target == null || image.texture != state.Target)
                state.StaticTexture = image.texture;
            Apply(image, state);
        }
        catch (Exception)
        {
        }
    }

    static State GetState(int key)
    {
        State state;
        if (!States.TryGetValue(key, out state))
        {
            state = new State();
            States[key] = state;
        }
        return state;
    }

    static int ResolveId(Texture texture)
    {
        int id;
        if (texture != null && TextureIds.TryGetValue(texture.GetInstanceID(), out id) && id > 0)
            return id;
        if (TryParseId(texture != null ? texture.name : null, out id))
            return id;
        return PendingId;
    }

    static void Apply(RawImage image, State state)
    {
        if (!state.IsCenter || state.MusicId <= 0)
        {
            Stop(image, state);
            return;
        }

        int musicId = Normalize(state.MusicId);
        string path = FindVideo(musicId);
        if (string.IsNullOrEmpty(path))
        {
            Stop(image, state);
            return;
        }

        if (state.Player != null && state.CurrentPath == path && state.Player.isPlaying && image.texture == state.Target)
            return;

        Play(image, state, path);
    }

    static void Play(RawImage image, State state, string path)
    {
        if (state.Target == null)
        {
            state.Target = new RenderTexture(RenderSize, RenderSize, 0);
            state.Target.name = "AnimatedJacketRT";
            state.Target.Create();
        }

        VideoPlayer player = state.Player;
        if (player == null)
        {
            GameObject host = new GameObject("AnimatedJacketPlayer");
            host.transform.SetParent(image.transform, false);
            player = host.AddComponent<VideoPlayer>();
            player.playOnAwake = false;
            player.isLooping = true;
            player.waitForFirstFrame = true;
            player.skipOnDrop = true;
            player.renderMode = VideoRenderMode.RenderTexture;
            player.audioOutputMode = VideoAudioOutputMode.None;
            player.aspectRatio = VideoAspectRatio.Stretch;
            player.errorReceived += OnError;
            player.prepareCompleted += OnPrepared;
            player.loopPointReached += OnLoop;
            state.Player = player;
        }

        player.enabled = true;
        player.isLooping = true;
        player.targetTexture = state.Target;
        player.url = path.Replace('\\', '/');
        state.CurrentPath = path;
        player.Prepare();
    }

    static void OnPrepared(VideoPlayer player)
    {
        try
        {
            player.isLooping = true;
            player.Play();
            State state = FindState(player);
            if (state == null)
                return;
            RawImage image = player.transform.parent.GetComponent<RawImage>();
            if (image != null && state.Target != null)
                image.texture = state.Target;
        }
        catch (Exception)
        {
        }
    }

    static void OnLoop(VideoPlayer player)
    {
        if (!player.isLooping)
        {
            player.isLooping = true;
            player.Play();
        }
    }

    static void OnError(VideoPlayer player, string message)
    {
        State state = FindState(player);
        if (state == null)
            return;
        RawImage image = player.transform.parent != null ? player.transform.parent.GetComponent<RawImage>() : null;
        Stop(image, state);
    }

    static State FindState(VideoPlayer player)
    {
        foreach (KeyValuePair<int, State> pair in States)
        {
            if (pair.Value.Player == player)
                return pair.Value;
        }
        return null;
    }

    static void Stop(RawImage image, State state)
    {
        if (state.Player != null)
        {
            state.Player.Stop();
            state.CurrentPath = null;
        }
        if (image != null && state.StaticTexture != null && (state.Target == null || image.texture == state.Target || image.texture == null))
            image.texture = state.StaticTexture;
    }

    static bool TryParseId(string text, out int musicId)
    {
        musicId = 0;
        if (string.IsNullOrEmpty(text) || text == "AnimatedJacketRT")
            return false;
        Match match = JacketId.Match(text);
        if (!match.Success)
            return false;
        return int.TryParse(match.Groups[1].Value, out musicId) && musicId > 0;
    }

    static int Normalize(int musicId)
    {
        if (musicId >= 10000)
            return musicId % 10000;
        return musicId;
    }

    static string FindVideo(int musicId)
    {
        string fileName = "ui_jacket_" + musicId.ToString("D6") + "_animated.mp4";
        string[] directories = GetSearchDirectories();
        for (int i = 0; i < directories.Length; i++)
        {
            string path = Path.Combine(directories[i], fileName);
            if (File.Exists(path))
                return path;
        }
        return null;
    }

    static string[] GetSearchDirectories()
    {
        if (SearchDirectories != null && SearchDirectories.Length > 0)
            return SearchDirectories;

        var found = new List<string>();
        try
        {
            string root = Application.streamingAssetsPath;
            if (Directory.Exists(root))
            {
                string[] children = Directory.GetDirectories(root);
                Array.Sort(children, StringComparer.OrdinalIgnoreCase);
                for (int i = children.Length - 1; i >= 0; i--)
                {
                    string dir = Path.Combine(children[i], "AssetBundleImages", "jacket_animated");
                    if (Directory.Exists(dir))
                        found.Add(dir);
                }
            }
            string direct = Path.Combine(root, "AssetBundleImages", "jacket_animated");
            if (Directory.Exists(direct))
                found.Add(direct);
        }
        catch (Exception)
        {
        }

        SearchDirectories = found.ToArray();
        return SearchDirectories;
    }
}
