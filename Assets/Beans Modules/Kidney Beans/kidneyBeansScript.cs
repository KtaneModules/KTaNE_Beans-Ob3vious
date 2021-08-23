using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class kidneyBeansScript : MonoBehaviour {

	public KMAudio Audio;
	public AudioClip[] sounds;
	public KMBombInfo BombInfo;
	public KMSelectable[] Beans;
	public GameObject Text;
	public GameObject Statuslight;
	public KMBombModule Module;

	private int[] beanArray = new int[9];
	private float[] offset = new float[9];
	private int[][] colours = new int[][] { new int[] { 255, 127, 63 }, new int[] { 0, 0, 0 }, new int[] { 0, 0, 0 } };
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
				Debug.LogFormat("[Kidney Beans #{0}] They know...", _moduleID);
				Module.HandleStrike();
			}
			else
			{
				int solution = -1;
				bool check = true;
				for (int i = 0; i < 9 && check; i++)
				{
					int match = 0;
					for (int j = 0; j < 3; j++)
						if (beanArray[(i / 3) * 3 + j] == beanArray[i % 3 + j * 3] || !beansafe[(i / 3) * 3 + j] || !beansafe[(i % 3) + j * 3])
							match++;
					if (match >= 2 && beanArray[i] != 0 && beansafe[i])
					{
						solution = i;
						check = false;
					}
				}
				if (solution != -1)
				{
					int match = 0;
					for (int j = 0; j < 3; j++)
						if (beanArray[(pos / 3) * 3 + j] == beanArray[pos % 3 + j * 3] || !beansafe[(pos / 3) * 3 + j] || !beansafe[(pos % 3) + j * 3])
							match++;
					if (!beansafe[pos] || match < 2 || beanArray[pos] == 0)
					{
						Debug.LogFormat("[Kidney Beans #{0}] Bean {1} wasn't really edible.", _moduleID, pos + 1);
						Module.HandleStrike();
						StartCoroutine(Strike());
					}
					else
						Debug.LogFormat("[Kidney Beans #{0}] Bean {1} was a correct bean to eat.", _moduleID, pos + 1);
				}
				else
					Debug.LogFormat("[Kidney Beans #{0}] There was no valid bean, so you will not get a strike.", _moduleID);
				beansafe[pos] = false;
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
		string[] colour = { " red", " maroon", " dark red" };
		Text.GetComponent<TextMesh>().text = (pos + 1) + colour[beanArray[pos]];
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

		bool ready = false;
		int[] solution = new int[3];
		while (!ready)
		{
			bool[] validpos = { true, true, true, true, true, true, true, true, true };
			int eaten = 0;
			for (int i = 0; i < 9; i++)
				beanArray[i] = Rnd.Range(0, 3);
			for (int x = 0; x < 3; x++)
				for (int i = 0; i < 9; i++)
				{
					int match = 0;
					for (int j = 0; j < 3; j++)
						if (beanArray[(i / 3) * 3 + j] == beanArray[i % 3 + j * 3] || !validpos[(i / 3) * 3 + j] || !validpos[(i % 3) + j * 3])
							match++;
					if (match >= 2 && eaten != 3 && beanArray[i] != 0 && validpos[i])
					{
						validpos[i] = false;
						solution[eaten] = i;
						eaten++;
					}
					if (eaten == 3)
						ready = true;
				}
		}
		Debug.LogFormat("[Kidney Beans #{0}] The beans are: {1}.", _moduleID, beanArray.Select(x => "rmd"[x].ToString()).Join(", "));
		Debug.LogFormat("[Kidney Beans #{0}] Potential beans to eat in order are: {1}.", _moduleID, solution.Select(x => x + 1).Join(", "));
		for (int i = 0; i < 9; i++)
		{
			offset[i] = Rnd.Range(0f, 360f);
			Beans[i].GetComponent<MeshRenderer>().material.color = new Color(colours[0][beanArray[i]] / 255f, colours[1][beanArray[i]] / 255f, colours[2][beanArray[i]] / 255f);
			Beans[i].transform.localEulerAngles = new Vector3(0f, offset[i], 0f);
			beansafe[i] = true;
		}
	}

	private IEnumerator Strike()
	{
		Statuslight.GetComponent<MeshRenderer>().material.color = new Color(colours[0][0] / 255f, colours[1][0] / 255f, colours[2][0] / 255f);
		yield return new WaitForSeconds(0.5f);
		if (eatenbeans < 3)
			Statuslight.GetComponent<MeshRenderer>().material.color = new Color(colours[0][2] / 255f, colours[1][2] / 255f, colours[2][2] / 255f);
	}

	private void Solve()
	{
		Statuslight.GetComponent<MeshRenderer>().material.color = new Color(colours[0][1] / 255f, colours[1][1] / 255f, colours[2][1] / 255f);
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
		for (int x = 0; x < 3; x++)
			for (int i = 0; i < 9 && eatenbeans != 3; i++)
			{
				int match = 0;
				for (int j = 0; j < 3; j++)
					if (beanArray[(i / 3) * 3 + j] == beanArray[i % 3 + j * 3] || !beansafe[(i / 3) * 3 + j] || !beansafe[(i % 3) + j * 3])
						match++;
				if (match >= 2 && beanArray[i] != 0 && beansafe[i])
				{
					Beans[i].OnInteract();
					yield return null;
				}
			}
		for (int i = 0; i < 9 && eatenbeans != 3; i++)
            if (Beans[i].transform.localScale.x > 0.01f)
            {
				Beans[i].OnInteract();
				yield return null;
			}
	}
}
