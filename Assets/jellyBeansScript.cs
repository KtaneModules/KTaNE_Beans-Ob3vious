using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class jellyBeansScript : MonoBehaviour {

	public KMAudio Audio;
	public AudioClip[] sounds;
	public KMBombInfo BombInfo;
	public KMSelectable[] Beans;
	public GameObject Text;
	public GameObject Statuslight;
	public KMBombModule Module;

	private int[] beanArray = new int[25];
	private bool[] beansafe = new bool[25];
	private bool[] beaneaten = new bool[25];
	private float[] offset = new float[25];
	private bool solved = false;

	static int _moduleIdCounter = 1;
	int _moduleID = 0;

	private KMSelectable.OnInteractHandler BeanPressed(int pos)
	{
		return delegate
		{
			Audio.PlaySoundAtTransform("Monch", Module.transform);
			if (solved)
			{
				Debug.LogFormat("[Jellybeans #{0}] You've had enough jellybeans.", _moduleID);
				Module.HandleStrike();
			}
			else
			{
				for (int i = 0; i < 25; i++)
				{
					if (!beaneaten[i])
					{
						switch (Value(beanArray[i]))
						{
							case 0:
								beansafe[(i + 20) % 25] = false;
								break;
							case 1:
								beansafe[(i + 1) % 5 + (i / 5) * 5] = false;
								break;
							case 2:
								beansafe[(i + 5) % 25] = false;
								break;
							case 3:
								beansafe[(i + 4) % 5 + (i / 5) * 5] = false;
								break;
						}
					}
				}
				if (!beansafe[pos])
				{
					Debug.LogFormat("[Jellybeans #{0}] Bean {1} is not edible. Maybe try another one?", _moduleID, pos + 1);
					StartCoroutine(Strike());
					Module.HandleStrike();
				}
				for (int i = 0; i < 25; i++)
				{
					beansafe[i] = true;
				}
				beaneaten[pos] = true;
				for (int i = 0; i < 25; i++)
				{
					if (!beaneaten[i])
					{
						switch (Value(beanArray[i]))
						{
							case 0:
								beansafe[(i + 20) % 25] = false;
								break;
							case 1:
								beansafe[(i + 1) % 5 + (i / 5) * 5] = false;
								break;
							case 2:
								beansafe[(i + 5) % 25] = false;
								break;
							case 3:
								beansafe[(i + 4) % 5 + (i / 5) * 5] = false;
								break;
						}
					}
				}
				bool cont = false;
				for (int i = 0; i < 25; i++)
				{
                    if (beansafe[i] && !beaneaten[i])
                    {
						cont = true;
                    }
					beansafe[i] = true;
				}
                if (!cont)
                {
					Module.HandlePass();
					Solve();
					solved = true;
                }
			}
			//Beans[pos].GetComponent<Renderer>().enabled = false;
			Beans[pos].transform.localScale = new Vector3(0f, 0f, 0f);
			return false;
		};
	}

	private void BeanHovered(int pos)
	{
		//string[] colour = { " orange", " yellow", " green" };
		Text.GetComponent<TextMesh>().text = (pos + 1).ToString("00") + " - " + (beanArray[pos] / 16) + ((beanArray[pos] / 4) % 4) + (beanArray[pos] % 4);
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

		int[] solution = { };
		int eaten = 0;
		bool[] eatenpos = new bool[25];
		while (eaten == 0)
		{
			for (int i = 0; i < 25; i++)
			{
				offset[i] = Rnd.Range(0f, 360f);
				beanArray[i] = Rnd.Range(0, 64);
				beansafe[i] = true;
			}
			bool ate = true;
			while (ate)
			{
				ate = false;
				for (int i = 0; i < 25; i++)
				{
					if (!eatenpos[i])
					{
						switch (Value(beanArray[i]))
						{
							case 0:
								beansafe[(i + 20) % 25] = false;
								break;
							case 1:
								beansafe[(i + 1) % 5 + (i / 5) * 5] = false;
								break;
							case 2:
								beansafe[(i + 5) % 25] = false;
								break;
							case 3:
								beansafe[(i + 4) % 5 + (i / 5) * 5] = false;
								break;
						}
					}
				}
				for (int i = 0; i < 25; i++)
				{
					if (beansafe[i] && !eatenpos[i])
					{
						eatenpos[i] = true;
						ate = true;
						solution = solution.Concat(new int[] { i }).ToArray();
						eaten++;
					}
					beansafe[i] = true;
				}
			}
		}
		Debug.LogFormat("[Jellybeans #{0}] All edible beans (and a potential order to eat them in) are the following: {1}.", _moduleID, solution.Select(x => x + 1).Join(", "));
		for (int i = 0; i < 25; i++)
		{
			Beans[i].GetComponent<MeshRenderer>().material.color = new Color((beanArray[i] / 16) / 3f, ((beanArray[i] / 4) % 4) / 3f, (beanArray[i] % 4) / 3f);
			Beans[i].transform.localEulerAngles = new Vector3(90f, offset[i], 0f);
		}
		Statuslight.GetComponent<MeshRenderer>().material.color = new Color(Rnd.Range(0, 4) / 3f, Rnd.Range(0, 4) / 3f, Rnd.Range(0, 4) / 3f);
	}

	private int Value (int num) {
		int[] value = { num / 16, (num / 4) % 4, num % 4 };
		int n = 0;
		for (int i = 0; i < 4; i++)
		{
			if (value.Count(x => x == i) > 1)
			{
				return i;
			}
			else if (value.Count(x => x == i) == 0)
			{
				n = i;
			}
		}
		return n;
	}

	private IEnumerator Strike()
	{
		Statuslight.GetComponent<MeshRenderer>().material.color = new Color(1f, 0f, 0f);
		yield return new WaitForSeconds(0.5f);
		if (!solved)
			Statuslight.GetComponent<MeshRenderer>().material.color = new Color(Rnd.Range(0, 4) / 3f, Rnd.Range(0, 4) / 3f, Rnd.Range(0, 4) / 3f);
	}

	private void Solve()
	{
		Statuslight.GetComponent<MeshRenderer>().material.color = new Color(0f, 1f, 0f);
	}

#pragma warning disable 414
	private string TwitchHelpMessage = "'!{0} cycle' to cycle through the beans, '!{0} 1' to eat a bean in reading order (indexed at 1). Note that you cannot press an already eaten bean. e.g. '!{0} 1 25 12 17'";
#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		yield return null;
		command = command.ToLowerInvariant();
		string[] cmds = command.Split(' ');
		if (command == "cycle")
		{
			for (int i = 0; i < 25; i++)
			{
				while (i < 25 && Beans[i].transform.localScale.x < 0.001f)
				{
					i++;
				}
				if (i != 25)
				{
					Beans[i].OnHighlight();
					yield return new WaitForSeconds(0.9f);
					Beans[i].OnHighlightEnded();
					yield return new WaitForSeconds(0.1f);
				}
			}
		}
		else
		{
			string[] validCommands = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25" };
			for (int i = 0; i < cmds.Length; i++)
			{
				if (!validCommands.Contains(cmds[i]))
				{
					yield return "sendtochaterror Invalid command.";
					yield break;
				}
			}
			for (int i = 0; !solved && i < cmds.Length; i++)
			{
				yield return null;
				for (int j = 0; j < validCommands.Length; j++)
				{
					if (cmds[i] == validCommands[j] && Beans[j].transform.localScale.x > 0.001f) { Beans[j].OnInteract(); }
				}
				yield return new WaitForSeconds(0.5f);
			}
			yield return "strike";
			yield return "solve";
		}
		yield return null;
	}
	
	IEnumerator TwitchHandleForcedSolve()
	{
		yield return true;
		while (!solved)
		{
			for (int i = 0; i < 25; i++)
			{
				if (!beaneaten[i])
				{
					switch (Value(beanArray[i]))
					{
						case 0:
							beansafe[(i + 20) % 25] = false;
							break;
						case 1:
							beansafe[(i + 1) % 5 + (i / 5) * 5] = false;
							break;
						case 2:
							beansafe[(i + 5) % 25] = false;
							break;
						case 3:
							beansafe[(i + 4) % 5 + (i / 5) * 5] = false;
							break;
					}
				}
			}
			bool next = false;
			for (int i = 0; i < 25; i++)
			{
				if (beansafe[i] && !beaneaten[i] && !next)
				{
					Beans[i].OnInteract();
					yield return true;
					next = true;
				}
				beansafe[i] = true;
			}
		}
	}
}
