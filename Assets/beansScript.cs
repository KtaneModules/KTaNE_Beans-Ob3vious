using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class beansScript : MonoBehaviour {

	public KMAudio Audio;
	public AudioClip[] sounds;
	public KMBombInfo BombInfo;
	public KMSelectable[] Beans;
	public GameObject Text;
	public KMBombModule Module;

	private int[] beanArray = new int[9];
	private float[] offset = new float[9];
	private int[] timeoffset = new int[9];
	private int[][] colours = new int[][] { new int[] { 255, 255, 0 }, new int[] { 112, 192, 255 }, new int[] { 0, 0, 0 } };
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
				Debug.LogFormat("[Beans #{0}] They know...", _moduleID);
				Module.HandleStrike();
			}
			else
			{
				int solution = 0;
				bool check = true;
				for (int i = 0; i < 9 && check; i++)
				{
					if (beansafe[i])
					{
						solution = i;
						check = false;
					}
				}
				if (!beansafe[pos])
				{
					bool[] valid = { false, false, true, false, true, false, true, true, false, true, false, true };
					if (valid[beanArray[pos] + 6 * (pos % 2)])
						Debug.LogFormat("[Beans #{0}] Eating bean {1} became unhygienic, remember?", _moduleID, pos + 1);
					else
						Debug.LogFormat("[Beans #{0}] Bean {1} wasn't really edible.", _moduleID, pos + 1);
					Module.HandleStrike();
				}
				else if (pos != solution)
				{
					Debug.LogFormat("[Beans #{0}] Why did you eat bean {1} when bean {2} was perfectly available?", _moduleID, pos + 1, solution + 1);
					Module.HandleStrike();
				}
				beansafe[pos] = false;
				if (pos / 3 != 0)
				{
					beansafe[pos - 3] = false;
				}
				if (pos / 3 != 2)
				{
					beansafe[pos + 3] = false;
				}
				if (pos % 3 != 0)
				{
					beansafe[pos - 1] = false;
				}
				if (pos % 3 != 2)
				{
					beansafe[pos + 1] = false;
				}
				eatenbeans++;
				if (eatenbeans == 3)
				{
					Module.HandlePass();
				}
			}
			//Beans[pos].GetComponent<Renderer>().enabled = false;
			Beans[pos].transform.localScale = new Vector3(0f, 0f, 0f);
			return false;
		};
	}

	private void BeanHovered(int pos)
	{
		string[] colour = { " orange", " yellow", " green" };
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


		bool[] valid = { false, false, true, false, true, false, true, true, false, true, false, true };

		bool ready = false;
		int[] solution = new int[3];
		while (!ready)
		{
			bool[] validpos = { true, true, true, true, true, true, true, true, true };
			int eaten = 0;
			for (int i = 0; i < 9; i++)
			{
				offset[i] = Rnd.Range(0f, 360f);
				timeoffset[i] = Rnd.Range(0, 100);
				beanArray[i] = Rnd.Range(0, 6);
				if (valid[beanArray[i] + 6 * (i % 2)] && validpos[i] && eaten != 3)
				{
					if (i / 3 != 0)
					{
						validpos[i - 3] = false;
					}
					if (i / 3 != 2)
					{
						validpos[i + 3] = false;
					}
					if (i % 3 != 0)
					{
						validpos[i - 1] = false;
					}
					if (i % 3 != 2)
					{
						validpos[i + 1] = false;
					}
					solution[eaten] = i;
					eaten++;
				}
				if (eaten == 3)
				{
					ready = true;
				}
			}
		}
		Debug.LogFormat("[Beans #{0}] Beans to eat in order are: {1}.", _moduleID, solution.Select(x => x + 1).Join(", "));
		for (int i = 0; i < 9; i++)
		{
			Beans[i].GetComponent<MeshRenderer>().material.color = new Color(colours[0][beanArray[i] % 3] / 255f, colours[1][beanArray[i] % 3] / 255f, colours[2][beanArray[i] % 3] / 255f);
			beansafe[i] = valid[beanArray[i] + 6 * (i % 2)];
		}
		StartCoroutine(Wobble());
	}

	private IEnumerator Wobble()
	{
		float t = 0;
		while (true)
		{
			for (int i = 0; i < 9; i++)
			{
				if (beanArray[i] / 3 == 1)
				{
					Beans[i].transform.localEulerAngles = new Vector3(0f, Mathf.Sin(t + timeoffset[i]) * 15f + offset[i], 0f);
				}
				else
				{
					Beans[i].transform.localEulerAngles = new Vector3(0f, offset[i], 0f);
				}
			}
			t += 0.5f;
			yield return new WaitForSeconds(0.02f);
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
		{
            for (int i = 0; i < 9; i++)
            {
                while (i < 9 && Beans[i].transform.localScale.x < 0.01f)
                {
					i++;
                }
                if (i != 9)
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
			string validCommands = "123456789";
			command = command.Replace(" ", "");
			for (int i = 0; i < command.Length; i++)
			{
				if (!validCommands.Contains(command[i]))
				{
					yield return "sendtochaterror Invalid command.";
					yield break;
				}
			}
			for (int i = 0; eatenbeans != 3 && i < command.Length; i++)
			{
				yield return null;
				for (int j = 0; j < validCommands.Length; j++)
				{
					if (command[i] == validCommands[j] && Beans[j].transform.localScale.x > 0.01f) { Beans[j].OnInteract(); }
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
		for (int i = 0; i < 9 && eatenbeans != 3; i++)
		{
			if (beansafe[i])
			{
				Beans[i].OnInteract();
				yield return true;
			}
		}
		for (int i = 0; i < 9 && eatenbeans != 3; i++)
		{
            if (Beans[i].transform.localScale.x > 0.01f)
            {
				Beans[i].OnInteract();
				yield return true;
			}
		}
	}
}
