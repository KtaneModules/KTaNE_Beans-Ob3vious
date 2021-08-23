using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class soyBeansScript : MonoBehaviour {

	public KMAudio Audio;
	public AudioClip[] sounds;
	public KMBombInfo BombInfo;
	public KMSelectable[] Beans;
	public GameObject Text;
	public GameObject Statuslight;
	public KMBombModule Module;

	private int[] beanArray = new int[9];
	private float[] offset = new float[9];
	private int[][] colours = new int[][] { new int[] { 192, 192, 128 }, new int[] { 255, 128, 64 }, new int[] { 0, 64, 0 } };
	private bool[] beansafe = new bool[9];
	private int lastbean = 4;
	private int eatenbeans = 0;

	static int _moduleIdCounter = 1;
	int _moduleID = 0;

	private KMSelectable.OnInteractHandler BeanPressed(int pos)
	{
		return delegate
		{
			Audio.PlaySoundAtTransform("Monch", Module.transform);
			if (eatenbeans == 3)
			{
				Debug.LogFormat("[Soy Beans #{0}] They know...", _moduleID);
				Module.HandleStrike();
			}
			else
			{
				int xp = lastbean % 3, yp = lastbean / 3;
				yp = beanArray[xp + yp * 3];
				while (!beansafe[xp + yp * 3])
					yp = (yp + 2) % 3;
				xp = beanArray[xp + yp * 3];
				while (!beansafe[xp + yp * 3])
					xp = (xp + 2) % 3;
				int solution = xp + yp * 3;
				beansafe[pos] = false;
				lastbean = pos;
				if (pos != solution)
				{
					Debug.LogFormat("[Beans #{0}] Why did you eat bean {1}, when you were supposed to eat bean {2}?", _moduleID, pos + 1, solution + 1);
					Module.HandleStrike();
					StartCoroutine(Strike());
				}
				else
					Debug.LogFormat("[Soy Beans #{0}] Bean {1} was the correct bean to eat.", _moduleID, pos + 1);
				eatenbeans++;
				if (eatenbeans == 3)
				{
					Module.HandlePass();
					Solve();
				}
			}
			Beans[pos].transform.localScale = new Vector3(0f, 0f, 0f);
			return false;
		};
	}

	private void BeanHovered(int pos)
	{
		string[] colour = { " green", " beige", " brown" };
		Text.GetComponent<TextMesh>().text = (pos + 1) + colour[beanArray[pos] % 3];
	}

	private void BeanHoverEnded()
	{
		Text.GetComponent<TextMesh>().text = "";
	}

	void Awake () {
		_moduleID = _moduleIdCounter++;

		Text.GetComponent<TextMesh>().text = "";

		for (int i = 0; i < Beans.Length; i++)
		{
			Beans[i].OnInteract += BeanPressed(i);
			int x = i;
			Beans[i].OnHighlight += delegate { BeanHovered(x); return; };
			Beans[i].OnHighlightEnded += delegate { BeanHoverEnded(); return; };
		}

	}

	void Start () {
		int[] solution = new int[3];
		bool[] notsafe = new bool[9];
		int xp = 1, yp = 1;
        for (int i = 0; i < 9; i++)
			beanArray[i] = Rnd.Range(0, 3);
		for (int i = 0; i < 3; i++)
        {
			yp = beanArray[xp + yp * 3];
			while (notsafe[xp + yp * 3])
				yp = (yp + 2) % 3;
			xp = beanArray[xp + yp * 3];
            while (notsafe[xp + yp * 3])
				xp = (xp + 2) % 3;
			solution[i] = xp + yp * 3;
			notsafe[xp + yp * 3] = true;
		}
		Debug.LogFormat("[Soy Beans #{0}] The beans are: {1}.", _moduleID, beanArray.Select(x => "gbn"[x % 3].ToString()).Join(", "));
		Debug.LogFormat("[Soy Beans #{0}] Beans to eat in order are: {1}.", _moduleID, solution.Select(x => x + 1).Join(", "));
		for (int i = 0; i < 9; i++)
		{
			offset[i] = Rnd.Range(0f, 360f);
			Beans[i].GetComponent<MeshRenderer>().material.color = new Color(colours[0][beanArray[i]] / 255f, colours[1][beanArray[i]] / 255f, colours[2][beanArray[i]] / 255f);
			beansafe[i] = true;
			Beans[i].transform.localEulerAngles = new Vector3(0f, offset[i], 0f);
		}
	}

	private IEnumerator Strike()
	{
		Statuslight.GetComponent<MeshRenderer>().material.color = new Color(colours[0][2] / 255f, colours[1][2] / 255f, colours[2][2] / 255f);
		yield return new WaitForSeconds(0.5f);
		if (eatenbeans < 3)
			Statuslight.GetComponent<MeshRenderer>().material.color = new Color(colours[0][1] / 255f, colours[1][1] / 255f, colours[2][1] / 255f);
	}

	private void Solve()
	{
		Statuslight.GetComponent<MeshRenderer>().material.color = new Color(colours[0][0] / 255f, colours[1][0] / 255f, colours[2][0] / 255f);
	}

	private IEnumerator Cycle()
	{
		for (int i = 0; i < 9; i++)
		{
			while (i < 9 && Beans[i].transform.localScale.x < 0.01f)
				i++;
			if (i != 9)
			{
				Beans[i].OnHighlight();
				yield return new WaitForSeconds(0.9f);
				Beans[i].OnHighlightEnded();
				yield return new WaitForSeconds(0.1f);
			}
		}
	}

#pragma warning disable 414
	private string TwitchHelpMessage = "'!{0} cycle' to cycle through the beans, '!{0} 1' to eat a bean in reading order (indexed at 1). No need to use spaces. Note that you cannot press an already eaten bean. e.g. '!{0} 139'";
#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		yield return null;
		command = command.ToLowerInvariant();
		if (command == "cycle")
			StartCoroutine(Cycle());
		else
		{
			string validCommands = "123456789";
			command = command.Replace(" ", "");
			for (int i = 0; i < command.Length; i++)
				if (!validCommands.Contains(command[i]))
				{
					yield return "sendtochaterror Invalid command.";
					yield break;
				}
			yield return "strike";
			yield return "solve";
			for (int i = 0; eatenbeans != 3 && i < command.Length; i++)
			{
				yield return null;
				for (int j = 0; j < validCommands.Length; j++)
					if (command[i] == validCommands[j] && Beans[j].transform.localScale.x > 0.01f)
						Beans[j].OnInteract();
			}
		}
		yield return null;
	}

	IEnumerator TwitchHandleForcedSolve()
	{
		yield return true;
        while (eatenbeans != 3)
        {
			int xp = lastbean % 3, yp = lastbean / 3;
			yp = beanArray[xp + yp * 3];
			while (!beansafe[xp + yp * 3])
				yp = (yp + 2) % 3;
			xp = beanArray[xp + yp * 3];
			while (!beansafe[xp + yp * 3])
				xp = (xp + 2) % 3;
			Beans[xp + yp * 3].OnInteract();
			yield return null;
		}
	}
}
