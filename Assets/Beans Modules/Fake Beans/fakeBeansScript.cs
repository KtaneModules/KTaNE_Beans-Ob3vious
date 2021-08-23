using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class fakeBeansScript : MonoBehaviour {

	public KMAudio Audio;
	public AudioClip[] sounds;
	public KMBombInfo BombInfo;
	public KMSelectable[] Beans;
	public GameObject[] SqBeans;
	public GameObject Text;
	public GameObject Statuslight;
	public KMBombModule Module;

	private int[] beanArray = new int[9];
	private float[] offset = new float[9];
	private int[][] colours = new int[][] { new int[] { 255, 255, 0, 0, 255 }, new int[] { 112, 192, 255, 0, 0 }, new int[] { 0, 0, 0, 255, 191 } };
	private bool[] beansafe = new bool[9];
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
				Debug.LogFormat("[Fake Beans #{0}] They know...", _moduleID);
				Module.HandleStrike();
			}
			else
			{
				if (Solutionpos() != -1)
				{
					if (Solutionpos() != pos)
					{
						Debug.LogFormat("[Fake Beans #{0}] Why did you eat bean {1}, when you were supposed to eat bean {2}?", _moduleID, pos + 1, Solutionpos() + 1);
						Module.HandleStrike();
						StartCoroutine(Strike());
					}
					else
						Debug.LogFormat("[Fake Beans #{0}] Bean {1} was the correct bean to eat.", _moduleID, pos + 1);
				}
				else
					Debug.LogFormat("[Fake Beans #{0}] There was no valid bean, so you will not get a strike.", _moduleID);
				beansafe[pos] = false;
				eatenbeans++;
				if (eatenbeans == 3)
				{
					Module.HandlePass();
					Solve();
				}
			}
			Beans[pos].transform.localScale = new Vector3(0f, 0f, 0f);
			SqBeans[pos].transform.localScale = new Vector3(0f, 0f, 0f);
			return false;
		};
	}

	private void BeanHovered(int pos)
	{
		string[] colour = { " orange", " yellow", " green", " purple", " pink" };
		Text.GetComponent<TextMesh>().text = (pos + 1) + colour[beanArray[pos] % 5];
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

	void Start()
	{
		bool ready = false;
		int[] solution = new int[3];
		bool[] validpos = new bool[9];
		while (!ready)
		{
			ready = true;
			beansafe = new bool[] { true, true, true, true, true, true, true, true, true };
			int amount = Rnd.Range(3, 6);
			while (validpos.Count(x => x) < amount)
				validpos[Rnd.Range(0, 9)] = true;
			for (int i = 0; i < 9; i++)
				if (validpos[i])
					beanArray[i] = Rnd.Range(0, 3);
				else
					beanArray[i] = Rnd.Range(3, 10);
			for (int i = 0; i < 3 && ready; i++)
				if (Solutionpos() != -1)
				{
					solution[i] = Solutionpos();
					beansafe[Solutionpos()] = false;
				}
				else
					ready = false;
		}
		Debug.LogFormat("[Fake Beans #{0}] The beans are: {1}.", _moduleID, beanArray.Select(x => "oygpi"[x % 5].ToString() + (x >= 5 ? "r" : "")).Join(", "));
		Debug.LogFormat("[Fake Beans #{0}] Beans to eat in order are: {1}.", _moduleID, solution.Select(x => x + 1).Join(", "));
		for (int i = 0; i < 9; i++)
		{
			offset[i] = Rnd.Range(0f, 360f);
			if (beanArray[i] / 5 == 0)
			{
				Beans[i].GetComponent<MeshRenderer>().material.color = new Color(colours[0][beanArray[i] % 5] / 255f, colours[1][beanArray[i] % 5] / 255f, colours[2][beanArray[i] % 5] / 255f);
				SqBeans[i].GetComponent<MeshRenderer>().enabled = false;
				Beans[i].transform.localEulerAngles = new Vector3(0f, offset[i], 0f);
			}
			else
			{
				SqBeans[i].GetComponent<MeshRenderer>().material.color = new Color(colours[0][beanArray[i] % 5] / 255f, colours[1][beanArray[i] % 5] / 255f, colours[2][beanArray[i] % 5] / 255f);
				Beans[i].GetComponent<MeshRenderer>().enabled = false;
				SqBeans[i].transform.localEulerAngles = new Vector3(0f, offset[i], 0f);
			}
		}
		Statuslight.GetComponent<MeshRenderer>().material.color = new Color(colours[0][1] / 255f, colours[1][1] / 255f, colours[2][1] / 255f);
		beansafe = new bool[] { true, true, true, true, true, true, true, true, true };
	}

	private int Solutionpos()
	{
		switch (Enumerable.Range(0, 9).Count(x => beanArray[x] < 3 && beansafe[x]))
		{
			case 1:
				for (int i = 0; i < 9; i++)
					if (beanArray[i] < 3 && beansafe[i])
						return i;
				break;
			case 2:
				{
					int j = 0;
					for (int i = 0; i < 9; i++)
					{
						if (beanArray[i] < 3 && beansafe[i] && j == (Enumerable.Range(0, 9).Count(x => beanArray[x] > 4 && beansafe[x]) + 1) % 2)
							return i;
						else if (beanArray[i] < 3 && beansafe[i])
							j++;
					}
				}
				break;
			case 3:
				{
					int j = 0;
					for (int i = 0; i < 9; i++)
					{
						if (beanArray[i] < 3 && beansafe[i] && j == (Enumerable.Range(0, 9).Where(x => beanArray[x] < 3 && beansafe[x]).Sum(x => beanArray[x]) + 2) % 3)
							return i;
						else if (beanArray[i] < 3 && beansafe[i])
							j++;
					}
				}
				break;
			case 4:
				{
					int j = 0;
					for (int i = 0; i < 9; i++)
					{
						if (beanArray[i] < 3 && beansafe[i] && j == Enumerable.Range(0, 9).Count(x => beanArray[x] % 5 == 3 && beansafe[x]) % 2 + Enumerable.Range(0, 9).Count(x => beanArray[x] % 5 == 4 && beansafe[x]) % 2 * 2)
							return i;
						else if (beanArray[i] < 3 && beansafe[i])
							j++;
					}
				}
				break;
			case 5:
				{
					int[] cols = new int[3];
					for (int i = 0; i < 9; i++)
						if (beanArray[i] < 3 && beansafe[i])
							cols[beanArray[i]]++;
					if (Enumerable.Range(0, 9).Count(x => beanArray[x] < 3 && cols[beanArray[x]] == 1 && beansafe[x]) == 0)
						return -1;
					else if (Enumerable.Range(0, 9).Count(x => beanArray[x] < 3 && cols[beanArray[x]] == 1 && beansafe[x]) == 2)
					{
						int j = 0;
						for (int i = 0; i < 9; i++)
						{
							if (beanArray[i] < 3 && beansafe[i] && cols[beanArray[i]] == 1 && j == (Enumerable.Range(0, 9).Count(x => beanArray[x] > 4 && beansafe[x]) + 1) % 2)
								return i;
							else if (beanArray[i] < 3 && beansafe[i] && cols[beanArray[i]] == 1)
								j++;
						}
					}
					else
					{
						for (int i = 0; i < 9; i++)
							if (beanArray[i] < 3 && beansafe[i] && cols[beanArray[i]] == 1)
								return i;
					}
				}
				break;
		}
		return -1;
	}

	private IEnumerator Strike()
	{
		Statuslight.GetComponent<MeshRenderer>().material.color = new Color(colours[0][0] / 255f, colours[1][0] / 255f, colours[2][0] / 255f);
		yield return new WaitForSeconds(0.5f);
		if (eatenbeans < 3)
			Statuslight.GetComponent<MeshRenderer>().material.color = new Color(colours[0][1] / 255f, colours[1][1] / 255f, colours[2][1] / 255f);
	}

	private void Solve()
	{
		Statuslight.GetComponent<MeshRenderer>().material.color = new Color(colours[0][2] / 255f, colours[1][2] / 255f, colours[2][2] / 255f);
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
		for (int i = 0; i < 3 && eatenbeans < 3; i++)
		{
			if (Solutionpos() != -1)
				Beans[Solutionpos()].OnInteract();
			else
				for (int j = 0; j < 9; j++)
					if (beansafe[j])
					{
						Beans[j].OnInteract();
						j = 9;
					}
			yield return null;
		}
	}
}
