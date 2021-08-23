using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class coffeeBeansScript : MonoBehaviour {

	public KMAudio Audio;
	public AudioClip[] sounds;
	public KMBombInfo BombInfo;
	public KMSelectable[] Beans;
	public GameObject Text;
	public GameObject Statuslight;
	public KMBombModule Module;

	private int[] beanArray = new int[3];
	private bool[] beansafe = new bool[3];
	private float[] offset = new float[3];
	private float[] rotvel = new float[9];
	private int[][] colours = new int[][] { new int[] { 63, 31, 159 }, new int[] { 31, 15, 95 }, new int[] { 0, 0, 0 } };
	private List<int> moves = new List<int> { };
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
				Debug.LogFormat("[Coffee Beans #{0}] They know...", _moduleID);
				Module.HandleStrike();
			}
			else
			{
				int solution = 0;
                switch (eatenbeans)
                {
					case 0:
						solution = ((((moves.Select(x => x + 1).Sum() % 2) + 2 * (moves.Select(x => (x + 1) / 2).Sum() % 2)) % 4) % 3);
						break;
					case 1:
						solution = (Enumerable.Range(0, moves.Count() - 1).Select(x => ((((moves[x] + 1) ^ (moves[x + 1] + 1)) % 4) % 3) == 0).Count(x => x) % 2);
						if (beansafe.Take(solution + 1).Any(x => !x))
							solution++;
						break;
					case 2:
						solution = Array.IndexOf(beansafe, true);
						break;
				}
				beansafe[pos] = false;
                if (pos != solution)
				{
					Debug.LogFormat("[Beans #{0}] Why did you eat bean {1}, when you were supposed to eat bean {2}?", _moduleID, pos + 1, solution + 1);
					Module.HandleStrike();
					StartCoroutine(Strike());
				}
                else
					Debug.LogFormat("[Coffee Beans #{0}] Bean {1} was the correct bean to eat.", _moduleID, pos + 1);
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
		Text.GetComponent<TextMesh>().text = (pos + 1).ToString();
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
		int movecount = Rnd.Range(3, 6);
        for (int i = 0; i < movecount; i++)
			moves.Add(Rnd.Range(0, 3));
		solution[0] = ((((moves.Select(x => x + 1).Sum() % 2) + 2 * (moves.Select(x => (x + 1) / 2).Sum() % 2)) % 4) % 3);
		solution[1] = (Enumerable.Range(0, moves.Count() - 1).Select(x => ((((moves[x] + 1) ^ (moves[x + 1] + 1)) % 4) % 3) == 0).Count(x => x) % 2);
		if (solution[1] >= solution[0])
			solution[1]++;
		solution[2] = (6 - solution.Sum()) % 3;
		Debug.LogFormat("[Coffee Beans #{0}] The movements are are: {1}.", _moduleID, moves.Select(x => "hvd"[x % 3].ToString()).Join(", "));
		Debug.LogFormat("[Coffee Beans #{0}] Beans to eat in order are: {1}.", _moduleID, solution.Select(x => x + 1).Join(", "));
		beanArray = Enumerable.Range(0, 4).ToList().Shuffle().Take(3).ToArray();
		for (int i = 0; i < 3; i++)
		{
			rotvel[i] = Rnd.Range(-1f, 1f);
			offset[i] = Rnd.Range(0f, 360f);
			Beans[i].transform.localEulerAngles = new Vector3(0f, offset[i], 0f);
			Beans[i].transform.localPosition = new Vector3((beanArray[i] % 2) / 20f - 0.025f, 0.015f, (beanArray[i] / 2) / 20f - 0.025f);
			beansafe[i] = true;
		}
		StartCoroutine(Slide());
		StartCoroutine(Wobble());
	}

	private IEnumerator Slide()
    {
        while (true)
        {
            for (int i = 0; i < moves.Count(); i++)
            {
				int e = Enumerable.Range(0, 4).Where(x => !beanArray.Contains(x)).First();
				int b = e ^ (moves[i] + 1);
				int bn = Array.IndexOf(beanArray, b);
				beanArray[bn] = e;
                for (float t = 0; t < 1f; t += Time.deltaTime * 3f)
                {
					Beans[bn].transform.localPosition = Vector3.Lerp(new Vector3((b % 2) / 20f - 0.025f, 0.015f, (b / 2) / 20f - 0.025f), new Vector3((e % 2) / 20f - 0.025f, 0.015f, (e / 2) / 20f - 0.025f), t);
					yield return null;
                }
				Beans[bn].transform.localPosition = new Vector3((e % 2) / 20f - 0.025f, 0.015f, (e / 2) / 20f - 0.025f);
			}
			yield return new WaitForSeconds(0.5f);
        }
    }

	private IEnumerator Wobble()
	{
		float t = 0;
		while (true)
		{
			for (int i = 0; i < 3; i++)
				Beans[i].transform.localEulerAngles = new Vector3(0f, rotvel[i] * t * 15f + offset[i], 0f);
			t += 1f;
			yield return new WaitForSeconds(0.02f);
		}
	}

	private IEnumerator Strike()
	{
		Statuslight.GetComponent<MeshRenderer>().material.color = new Color(colours[0][1] / 255f, colours[1][1] / 255f, colours[2][1] / 255f);
		yield return new WaitForSeconds(0.5f);
		if (eatenbeans < 3)
			Statuslight.GetComponent<MeshRenderer>().material.color = new Color(colours[0][0] / 255f, colours[1][0] / 255f, colours[2][0] / 255f);
	}

	private void Solve()
	{
		Statuslight.GetComponent<MeshRenderer>().material.color = new Color(colours[0][2] / 255f, colours[1][2] / 255f, colours[2][2] / 255f);
	}

#pragma warning disable 414
	private string TwitchHelpMessage = "'!{0} 1' to eat the bean labeled 1. No need to use spaces. Note that you cannot press an already eaten bean. e.g. '!{0} 132'";
#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		yield return null;
		command = command.ToLowerInvariant();
		string validCommands = "123";
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
		yield return null;
	}

	IEnumerator TwitchHandleForcedSolve()
	{
		yield return true;
        while (eatenbeans != 3)
        {
			int solution = 0;
			switch (eatenbeans)
			{
				case 0:
					solution = ((((moves.Select(x => x + 1).Sum() % 2) + 2 * (moves.Select(x => (x + 1) / 2).Sum() % 2)) % 4) % 3);
					break;
				case 1:
					solution = (Enumerable.Range(0, moves.Count() - 1).Select(x => ((((moves[x] + 1) ^ (moves[x + 1] + 1)) % 4) % 3) == 0).Count(x => x) % 2);
					if (beansafe.Take(solution + 1).Any(x => !x))
						solution++;
					break;
				case 2:
					solution = Array.IndexOf(beansafe, true);
					break;
			}
			Beans[solution].OnInteract();
			yield return null;
		}
	}
}
