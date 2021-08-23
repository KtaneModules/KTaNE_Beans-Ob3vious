using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class beanSproutsScript : MonoBehaviour {

	public KMAudio Audio;
	public AudioClip[] sounds;
	public KMBombInfo BombInfo;
	public KMSelectable[] Beans;
	public GameObject[] BeanletsL;
	public GameObject[] BeanletsR;
	public GameObject Text;
	public GameObject Statuslight;
	public KMBombModule Module;

	private int[] beanArray = new int[9];
	private int[][] colours = new int[][] { new int[] { 255, 191, 0 }, new int[] { 223, 127, 0 }, new int[] { 191, 0, 0 } };
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
				Debug.LogFormat("[Bean Sprouts #{0}] They know...", _moduleID);
				Module.HandleStrike();
			}
			else
			{
				int solution = -1;
				bool check = true;
				for (int i = 0; i < 9 && check; i++)
					if (beansafe[i] && beanArray[i] == 4)
					{
						solution = i;
						check = false;
					}
				for (int i = 0; i < 9 && check; i++)
					if (beansafe[i] && Validcheck(i))
					{
						solution = i;
						check = false;
					}
				if (solution != -1)
				{
					if (!beansafe[pos] || !Validcheck(pos))
					{
						Debug.LogFormat("[Bean Sprouts #{0}] Sprout {1} wasn't really edible.", _moduleID, pos + 1);
						Module.HandleStrike();
						StartCoroutine(Strike());
					}
					else if (pos != solution)
					{
						Debug.LogFormat("[Bean Sprouts #{0}] Why did you eat sprout {1} when sprout {2} was perfectly available?", _moduleID, pos + 1, solution + 1);
						Module.HandleStrike();
						StartCoroutine(Strike());
					}
					else
						Debug.LogFormat("[Bean Sprouts #{0}] Sprout {1} was the correct sprout to eat.", _moduleID, pos + 1);
				}
				else
					Debug.LogFormat("[Bean Sprouts #{0}] There was no valid sprout, so you will not get a strike.", _moduleID);
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
		string[] colour = { " raw", " cooked", " burnt" };
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
		//It is likely to crash otherwise
		StartCoroutine(Generate());
	}

	private bool Validcheck(int index)
	{
		//rl cl bl
		//rn cn bn
		//rr cr br
		switch (beanArray[index])
		{
			case 0:
				int a = index, b = index;
				a = (a + 8) % 9;
				while (!beansafe[a])
					a = (a + 8) % 9;
				b = (b + 1) % 9;
				while (!beansafe[b])
					b = (b + 1) % 9;
				return beanArray[a] == beanArray[b];
			case 1:
				return Enumerable.Range(0, 9).Count(x => beanArray[x] / 3 == 0 && beansafe[x]) == Enumerable.Range(0, 9).Count(x => beanArray[x] / 3 == 2 && beansafe[x]);
			case 2:
				return Enumerable.Range(0, 9).Count(x => beanArray[x] == 2 && beansafe[x]) < 3;
			case 3:
				return index % 2 == 1;
			case 4:
				return true;
			case 5:
				return Enumerable.Range(0, 9).All(x => beanArray[x] % 3 != 1 || !beansafe[x]);
			case 6:
				return Enumerable.Range(0, 9).All(x => beanArray[x] / 3 != 1 || !beansafe[x]);
			case 7:
				int n = index + 1;
				while (n != 9 && !beansafe[n])
					n++;
				return (n == 9) ? true : !Validcheck(n);
			case 8:
				return eatenbeans == 1;
		}
		return false;
	}

	private IEnumerator Generate()
	{
		bool ready = false;
		int[] solution = new int[3];
		while (!ready)
		{
			for (int i = 0; i < 9; i++)
				beanArray[i] = Rnd.Range(0, 9);
			beansafe = new bool[] { true, true, true, true, true, true, true, true, true };
			eatenbeans = 0;
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 9 && eatenbeans == i; j++)
					if (beanArray[j] == 4 && beansafe[j])
					{
						solution[eatenbeans] = j;
						eatenbeans++;
						beansafe[j] = false;
					}
				for (int j = 0; j < 9 && eatenbeans == i; j++)
					if (Validcheck(j) && beansafe[j])
					{
						solution[eatenbeans] = j;
						eatenbeans++;
						beansafe[j] = false;
					}
			}
			if (eatenbeans == 3)
				ready = true;
			yield return null;
		}
		beansafe = new bool[] { true, true, true, true, true, true, true, true, true };
		eatenbeans = 0;
		Debug.LogFormat("[Bean Sprouts #{0}] The sprouts are: {1}.", _moduleID, beanArray.Select(x => "rcb"[x % 3].ToString() + "lnr"[x / 3].ToString()).Join(", "));
		Debug.LogFormat("[Bean Sprouts #{0}] Sprouts to eat in order are: {1}.", _moduleID, solution.Select(x => x + 1).Join(", "));
		for (int i = 0; i < 9; i++)
		{
			switch (beanArray[i] / 3)
			{
				case 0:
					BeanletsR[i].GetComponent<MeshRenderer>().enabled = false;
					break;
				case 1:
					BeanletsL[i].GetComponent<MeshRenderer>().enabled = false;
					BeanletsR[i].GetComponent<MeshRenderer>().enabled = false;
					break;
				case 2:
					BeanletsL[i].GetComponent<MeshRenderer>().enabled = false;
					break;
			}
			Beans[i].GetComponent<MeshRenderer>().material.color = new Color(colours[0][beanArray[i] % 3] / 255f, colours[1][beanArray[i] % 3] / 255f, colours[2][beanArray[i] % 3] / 255f);
		}
	}

	private IEnumerator Strike()
	{
		Statuslight.GetComponent<MeshRenderer>().material.color = new Color(colours[0][2] / 255f, colours[1][2] / 255f, colours[2][2] / 255f);
		yield return new WaitForSeconds(0.5f);
		if (eatenbeans < 3)
			Statuslight.GetComponent<MeshRenderer>().material.color = new Color(colours[0][0] / 255f, colours[1][0] / 255f, colours[2][0] / 255f);
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
	private string TwitchHelpMessage = "'!{0} cycle' to cycle through the beans, '!{0} 1' to eat a sprout in reading order (indexed at 1). No need to use spaces. Note that you cannot press an already eaten sprout. e.g. '!{0} 139'";
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
		for (int i = 0; i < 9 && eatenbeans != 3; i++)
			if (beansafe[i] && beanArray[i] == 4)
			{
				Beans[i].OnInteract();
				yield return null;
			}
        for (int j = eatenbeans; j < 3; j++)
			for (int i = 0; i < 9 && j == eatenbeans; i++)
				if (beansafe[i] && Validcheck(i))
				{
					Beans[i].OnInteract();
					yield return null;
				}
		for (int i = 0; i < 9 && eatenbeans != 3; i++)
            if (Beans[i].transform.localScale.x > 0.01f)
            {
				Beans[i].OnInteract();
				yield return null;
			}
	}
}
