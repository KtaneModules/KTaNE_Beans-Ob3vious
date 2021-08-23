using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class chilliBeansScript : MonoBehaviour {

	public KMAudio Audio;
	public AudioClip[] sounds;
	public KMBombInfo BombInfo;
	public KMSelectable[] Beans;
	public GameObject Text;
	public GameObject Statuslight;
	public KMBombModule Module;

	private int[] beanArray = new int[16];
	private int[] beanArray2 = new int[16];
	private float[] offset = new float[16];
	private int[][] colours = new int[][] { new int[] { 191, 191, 191, 255 }, new int[] { 127, 63, 0, 255 }, new int[] { 0, 0, 0, 255 } };
	private bool[] beansafe = new bool[16];
	private bool[] beanate = new bool[16];
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
				Debug.LogFormat("[Chilli Beans #{0}] They know...", _moduleID);
				Module.HandleStrike();
			}
			else
			{
				int solution = -1;
				bool check = true;
				for (int j = 0; j < 16; j++)
					if (beanate[j])
						beanArray[j] = 4;
				for (int i = 0; i < 16 && check; i++)
					if (beanArray[i] == beanArray.Min() && beansafe[i])
					{
						solution = i;
						check = false;
					}
				if (solution != -1)
				{
					if (beanArray[pos] != beanArray.Min() || !beansafe[pos])
					{
						Debug.LogFormat("[Chilli Beans #{0}] Bean {1} wasn't really edible.", _moduleID, pos + 1);
						Module.HandleStrike();
						StartCoroutine(Strike());
					}
					else if (pos != solution)
					{
						Debug.LogFormat("[Chilli Beans #{0}] Why did you eat bean {1} when bean {2} was perfectly available?", _moduleID, pos + 1, solution + 1);
						Module.HandleStrike();
						StartCoroutine(Strike());
					}
					else
						Debug.LogFormat("[Chilli Beans #{0}] Bean {1} was the correct bean to eat.", _moduleID, pos + 1);
				}
				else
					Debug.LogFormat("[Chilli Beans #{0}] There was no valid bean, so you will not get a strike.", _moduleID);
				for (int i = 0; i < 4; i++)
				{
					beansafe[pos % 4 + i * 4] = false;
					beansafe[(pos / 4) * 4 + i] = false;
				}
				beanate[pos] = true;
				eatenbeans++;
				for (int j = 0; j < 16; j++)
					beanArray[j]++;
				for (int j = 0; j < 16; j++)
					if (beanate[j])
						beanArray[j] = 0;
				while (beanArray.Any(x => x > 3))
				{
					for (int j = 0; j < 16; j++)
						if (beanArray[j] > 3)
						{
							beanArray[j] -= 4;
							if (j > 3)
								beanArray[j - 4]++;
							if (j % 4 > 0)
								beanArray[j - 1]++;
							if (j < 12)
								beanArray[j + 4]++;
							if (j % 4 < 3)
								beanArray[j + 1]++;
						}
					for (int j = 0; j < 16; j++)
						if (beanate[j])
							beanArray[j] = 0;
				}
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
		string[] colour = { " yellow", " orange", " red", " white" };
		Text.GetComponent<TextMesh>().text = (pos + 1) + colour[beanArray2[pos]];
	}

	private void BeanHoverEnded()
	{
		Text.GetComponent<TextMesh>().text = "";
	}

	void Awake () {
		_moduleID = _moduleIdCounter++;

		Text.GetComponent<TextMesh>().text = "";
		Statuslight.GetComponent<MeshRenderer>().material.color = new Color(colours[0][1] / 255f, colours[1][1] / 255f, colours[2][1] / 255f);

		for (int i = 0; i < Beans.Length; i++)
		{
			Beans[i].OnInteract += BeanPressed(i);
			int x = i;
			Beans[i].OnHighlight += delegate { BeanHovered(x); return; };
			Beans[i].OnHighlightEnded += delegate { BeanHoverEnded(); return; };
		}

	}

	void Start () {

		bool ready = false;
		int[] solution = new int[3];
		while (!ready)
		{
			bool[] validpos = { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true };
			bool[] eatenpos = new bool[16];
			int eaten = 0;
			for (int i = 0; i < 16; i++)
			{
				beanArray[i] = Rnd.Range(0, 4);
				beanArray2[i] = beanArray[i];
			}
			int[] temparray = new int[16];
			for (int i = 0; i < 16; i++)
				temparray[i] = beanArray[i];
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 16; j++)
					if (eatenpos[j])
						temparray[j] = 4;
				for (int j = 0; j < 16 && eaten == i; j++)
					if (temparray[j] == temparray.Min() && validpos[j])
					{
						for (int k = 0; k < 4; k++)
						{
							validpos[j % 4 + k * 4] = false;
							validpos[(j / 4) * 4 + k] = false;
						}
						eatenpos[j] = true;
						solution[eaten] = j;
						eaten++;
					}
				for (int j = 0; j < 16; j++)
					temparray[j]++;
				for (int j = 0; j < 16; j++)
					if (eatenpos[j])
						temparray[j] = 0;
				while (temparray.Any(x => x > 3))
				{
					for (int j = 0; j < 16; j++)
						if (temparray[j] > 3)
						{
							temparray[j] -= 4;
							if (j > 3)
								temparray[j - 4]++;
							if (j % 4 > 0)
								temparray[j - 1]++;
							if (j < 12)
								temparray[j + 4]++;
							if (j % 4 < 3)
								temparray[j + 1]++;
						}
					for (int j = 0; j < 16; j++)
						if (eatenpos[j])
							temparray[j] = 0;
				}
			}
			if (eaten == 3)
				ready = true;
		}
		Debug.LogFormat("[Chilli Beans #{0}] The beans are: {1}.", _moduleID, beanArray.Select(x => "yorw"[x].ToString()).Join(", "));
		Debug.LogFormat("[Chilli Beans #{0}] Beans to eat in order are: {1}.", _moduleID, solution.Select(x => x + 1).Join(", "));
		for (int i = 0; i < 16; i++)
		{
			offset[i] = Rnd.Range(0f, 360f);
			Beans[i].GetComponent<MeshRenderer>().material.color = new Color(colours[0][beanArray[i]] / 255f, colours[1][beanArray[i]] / 255f, colours[2][beanArray[i]] / 255f);
			Beans[i].transform.localEulerAngles = new Vector3(0f, offset[i], 0f);
			beansafe[i] = true;
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
		for (int i = 0; i < 16; i++)
		{
			while (i < 16 && Beans[i].transform.localScale.x < 0.001f)
				i++;
			if (i != 16)
			{
				Beans[i].OnHighlight();
				yield return new WaitForSeconds(0.9f);
				Beans[i].OnHighlightEnded();
				yield return new WaitForSeconds(0.1f);
			}
		}
	}

#pragma warning disable 414
	private string TwitchHelpMessage = "'!{0} cycle' to cycle through the beans, '!{0} 1' to eat a bean in reading order (indexed at 1). Note that you cannot press an already eaten bean. e.g. '!{0} 1 7 12'";
#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		yield return null;
		command = command.ToLowerInvariant();
		string[] cmds = command.Split(' ');
		if (command == "cycle")
			StartCoroutine(Cycle());
		else
		{
			string[] validCommands = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16" };
			for (int i = 0; i < cmds.Length; i++)
				if (!validCommands.Contains(cmds[i]))
				{
					yield return "sendtochaterror Invalid command.";
					yield break;
				}
			yield return "strike";
			yield return "solve";
			for (int i = 0; eatenbeans != 3 && i < cmds.Length; i++)
			{
				yield return null;
				for (int j = 0; j < validCommands.Length; j++)
					if (cmds[i] == validCommands[j] && Beans[j].transform.localScale.x > 0.001f) 
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
			for (int i = 0; i < 3; i++)
				for (int j = 0; j < 16 && eatenbeans == i; j++)
					if (beanArray[j] == Enumerable.Range(0, 16).Where(x => beansafe[x]).Select(x => beanArray[x]).Min() && beansafe[j])
					{
						Beans[j].OnInteract();
						yield return null;
					}
			for (int i = 0; i < 16; i++)
				if (!beanate[i] && eatenbeans != 3)
				{
					Beans[i].OnInteract();
					yield return null;
				}
		}
	}
}
