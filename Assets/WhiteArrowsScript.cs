using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class WhiteArrowsScript : MonoBehaviour
{

    public KMAudio Audio;
    public KMBombInfo Bomb;
	public KMBombModule Module;
    public KMColorblindMode Colorblind;

    public KMSelectable[] buttons;
    public GameObject numDisplay;
    public GameObject colorblindText;
	public Material[] ColorArrows;
	public MeshRenderer[] ArrowHeads;

    bool Animating = false;
	private bool ColorBlindActive = false;
	bool Playable = false;
	int[] NumberAssist = {0, 0};
	int Stage = 0;
	Coroutine Timer;
	string[] Colors = {"Blue", "Red", "Yellow", "Green", "Purple", "Orange", "Cyan", "Teal"};
	string[] Orientation = {"Up", "Right", "Down", "Left"};
	
	private int[][][] Mazes = new int[][][]{
		new int[][]{
			new int[] {3, 3, 0, 2, 1, 2, 2},
			new int[] {0, 1, 3, 0, 2, 0, 3},
			new int[] {1, 0, 2, 1, 3, 3, 1},
			new int[] {2, 2, 1, 3, 0, 1, 0}
		},
		new int[][]{
			new int[] {2, 2, 0, 2, 3, 0, 1},
			new int[] {3, 0, 1, 3, 2, 3, 0},
			new int[] {0, 1, 2, 0, 0, 1, 2},
			new int[] {1, 3, 3, 1, 1, 2, 3}
		},
		new int[][]{
			new int[] {3, 2, 1, 2, 2, 3, 3},
			new int[] {0, 3, 2, 1, 3, 0, 0},
			new int[] {1, 0, 3, 0, 0, 2, 1},
			new int[] {2, 1, 0, 3, 1, 1, 2}
		},
		new int[][]{
			new int[] {3, 1, 2, 0, 2, 2, 1},
			new int[] {2, 2, 0, 2, 1, 3, 3},
			new int[] {1, 3, 1, 3, 0, 1, 0},
			new int[] {0, 0, 3, 1, 3, 0, 2}
		},
		new int[][]{
			new int[] {0, 3, 0, 3, 1, 3, 3},
			new int[] {1, 2, 2, 1, 3, 1, 0},
			new int[] {2, 1, 3, 2, 2, 2, 1},
			new int[] {3, 0, 1, 0, 0, 0, 2}
		},
		new int[][]{
			new int[] {1, 2, 1, 2, 1, 3, 1},
			new int[] {3, 3, 2, 3, 0, 0, 3},
			new int[] {2, 1, 3, 0, 2, 1, 0},
			new int[] {0, 0, 0, 1, 3, 2, 2}
		},
		new int[][]{
			new int[] {0, 0, 3, 1, 1, 2, 2},
			new int[] {1, 2, 2, 2, 2, 3, 1},
			new int[] {2, 3, 1, 0, 3, 0, 0},
			new int[] {3, 1, 0, 3, 0, 1, 3}
		},
		new int[][]{
			new int[] {3, 0, 1, 0, 1, 1, 2},
			new int[] {1, 2, 0, 3, 0, 2, 0},
			new int[] {2, 1, 3, 2, 3, 0, 1},
			new int[] {0, 3, 2, 1, 2, 3, 3}
		},
	};
	
	#pragma warning disable 0649
    private bool TwitchPlaysActive;
    #pragma warning restore 0649
	float waitTime = 8f;
	
    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool ModuleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        for (int a = 0; a < buttons.Count(); a++)
        {
            int ArrowPos = a;
            buttons[ArrowPos].OnInteract += delegate
            {
                PressArrow(ArrowPos);
				return false;
            };
        }
		Module.OnActivate += WhiteArrowsOnTP;
	}
	
	void WhiteArrowsOnTP()
	{
		waitTime = TwitchPlaysActive ? 18.5f : 8f;
	}
	
	void PressArrow(int ArrowPos)
	{
		buttons[ArrowPos].AddInteractionPunch(0.25f);
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		if (Playable && !Animating && !ModuleSolved)
		{
			if (ArrowPos == Mazes[NumberAssist[1]][NumberAssist[0]][Stage])
			{
				Stage++;
				if (Timer != null)
				{
					StopCoroutine(Timer);
				}
				Debug.LogFormat("[White Arrows #{0}] You pressed {1}. That was correct", moduleId, Orientation[ArrowPos].ToLower());
				if (Stage != 7)
				{
					StartCoroutine(Blacken());
					Timer = StartCoroutine(TimerShenanigans());
				}
				else
				{
					Timer = StartCoroutine(Victory());
				}
			}
			
			else
			{
				if (Timer != null)
				{
					StopCoroutine(Timer);
				}
				Debug.LogFormat("[White Arrows #{0}] You pressed {1}. That was incorrect", moduleId, Orientation[ArrowPos].ToLower());
				Module.HandleStrike();
				StartCoroutine(Blacken());
				Stage = 0;
			}
		}
	}
	
	IEnumerator TimerShenanigans()
	{
		yield return new WaitForSecondsRealtime(waitTime);
		Module.HandleStrike();
		Debug.LogFormat("[White Arrows #{0}] Timer ran out. Module striked.", moduleId);
		StartCoroutine(Blacken());
		Stage = 0;
	}
	
	IEnumerator Blacken()
	{
		Playable = false;
		for (int x = 0; x < 4; x++)
		{
			ArrowHeads[x].material = ColorArrows[9];
		}
		yield return new WaitForSecondsRealtime(0.5f);
		Generate();
		Playable = true;
	}
	
	IEnumerator Victory()
    {
		Animating = true;
		Debug.LogFormat("[White Arrows #{0}] The module is solved", moduleId);
		for (int x = 0; x < 4; x++)
		{
			ArrowHeads[x].material = ColorArrows[9];
		}
		yield return new WaitForSecondsRealtime(0.5f);
		for (int y = 0; y < 4; y++)
		{
			ArrowHeads[y].material = ColorArrows[8];
		}
        for (int i = 0; i < 100; i++)
        {
            int rand1 = UnityEngine.Random.Range(0, 10);
            if (i < 50)
            {
                numDisplay.GetComponent<TextMesh>().text = rand1 + "";
            }
            else
            {
                numDisplay.GetComponent<TextMesh>().text = "G" + rand1;
            }
            yield return new WaitForSeconds(0.025f);
        }
        numDisplay.GetComponent<TextMesh>().text = "GG";
        Animating = false;
		ModuleSolved = true;
        Module.HandlePass();
    }


    void Start()
    {
        Module.OnActivate += Generate;
		StartCoroutine(ColorblindDelay());
    }
	
	private IEnumerator ColorblindDelay()
    {
        yield return new WaitForSeconds(0.5f);
        ColorBlindActive = Colorblind.ColorblindModeActive;
        if (ColorBlindActive)
        {
            Debug.LogFormat("[White Arrows #{0}] Colorblind mode active!", moduleId);
            colorblindText.SetActive(true);
        }
    }

    void Generate()
	{
		NumberAssist[0] = UnityEngine.Random.Range(0, 4);
		for (int x = 0; x < 4; x++)
		{
			if (x == NumberAssist[0])
			{
				NumberAssist[1] = UnityEngine.Random.Range(0, 8);
				ArrowHeads[x].material = ColorArrows[NumberAssist[1]];
			}
			
			else
			{
				ArrowHeads[x].material = ColorArrows[8];
			}
		}
		Debug.LogFormat("[White Arrows #{0}] Current Stage: {1}", moduleId, (Stage + 1).ToString());
		Debug.LogFormat("[White Arrows #{0}] The odd arrow: {1} {2} Arrow", moduleId, Colors[NumberAssist[1]], Orientation[NumberAssist[0]]);
		Debug.LogFormat("[White Arrows #{0}] Correct arrow to press: {1}", moduleId, Orientation[Mazes[NumberAssist[1]][NumberAssist[0]][Stage]]);
		if (!Playable)
		{
			Playable = true;
		}
	}
	
	//twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} up/right/down/left [Presses the specified arrow button] | Words can be substituted as one letter (Ex. right as r)";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*up\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*u\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
			yield return "solve";
            buttons[0].OnInteract();
        }
        if (Regex.IsMatch(command, @"^\s*down\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*d\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
			yield return "solve";;
            buttons[2].OnInteract();
        }
        if (Regex.IsMatch(command, @"^\s*left\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*l\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
			yield return "solve";
            buttons[3].OnInteract();
        }
        if (Regex.IsMatch(command, @"^\s*right\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*r\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
			yield return "solve";
            buttons[1].OnInteract();
        }
    }
}