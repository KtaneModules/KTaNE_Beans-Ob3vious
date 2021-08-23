using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class abeanminationScript : MonoBehaviour {

	public KMAudio Audio;
	public AudioClip[] sounds;
	public KMBombInfo BombInfo;
	public KMSelectable[] Beans;
	public KMSelectable[] LongBeans;
	public KMSelectable[] BeanSprouts;
	public KMSelectable[] BigBeans;
	public Material[] Materials;
	public GameObject[] Longbeanlets;
	public GameObject[] Jellybeans;
	public GameObject[] Kidneybeans;
	public GameObject[] Sproutbeanlets;
	public GameObject[] Sqbeans;
	public GameObject[] Boozlebeans;
	public TextMesh[] Boozletexts;
	public GameObject Text;
	public GameObject Statuslight;
	public KMBombModule Module;

	private int[] beanArray = new int[9];
	private float[] offset = new float[9];
	private int[] timeoffset = new int[9];
	private int[][] colours = new int[][] { new int[] { 255, 255, 0, 0, 255 }, new int[] { 112, 192, 255, 0, 0 }, new int[] { 0, 0, 0, 255, 191 } };
	private int[][] rottencolours = new int[][] { new int[] { 128, 64 }, new int[] { 64, 64 }, new int[] { 0, 64 } };
	private int[][] kidneycolours = new int[][] { new int[] { 255, 127, 63 }, new int[] { 0, 0, 0 }, new int[] { 0, 0, 0 } };
	private int[][] chillicolours = new int[][] { new int[] { 191, 191, 191, 255 }, new int[] { 127, 63, 0, 255 }, new int[] { 0, 0, 0, 255 } };
	private int[][] sproutcolours = new int[][] { new int[] { 255, 191, 0 }, new int[] { 223, 127, 0 }, new int[] { 191, 0, 0 } };
	private int[][] bigcolours = new int[][] { new int[] { 192, 192, 0 }, new int[] { 84, 144, 192 }, new int[] { 0, 0, 0 } };
	private int[][] boozlecolours = new int[][] { new int[] { 0, 0, 0 }, new int[] { 1, 1, 0 }, new int[] { 2, 1, 0 }, new int[] { 1, 2, 0 }, new int[] { 2, 2, 0 }, new int[] { 3, 1, 0 }, new int[] { 1, 3, 0 }, new int[] { 3, 2, 0 }, new int[] { 2, 3, 0 }, new int[] { 3, 3, 0 }, new int[] { 2, 2, 1 }, new int[] { 3, 2, 1 }, new int[] { 2, 3, 1 }, new int[] { 3, 3, 1 }, new int[] { 3, 3, 2 }, new int[] { 3, 3, 3 } };
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
				Debug.LogFormat("[Abeanmination #{0}] They know...", _moduleID);
				Module.HandleStrike();
			}
			else
			{
				int solution = -1;
				bool check = true;
				for (int i = 0; i < 9 && check; i++)
				{
					if (beansafe[i])
					{
						solution = i;
						check = false;
					}
				}
				if (solution != -1)
				{
					if (!beansafe[pos])
					{
						bool[] valid = { false, false, true, false, true, false, true, true, false, true, false, true };
						if (valid[beanArray[pos] + 6 * (pos % 2)])
							Debug.LogFormat("[Abeanmination #{0}] Eating bean {1} became unhygienic, remember?", _moduleID, pos + 1);
						else
							Debug.LogFormat("[Abeanmination #{0}] Bean {1} wasn't really edible.", _moduleID, pos + 1);
						Module.HandleStrike();
						StartCoroutine(Strike());
					}
					else if (pos != solution)
					{
						Debug.LogFormat("[Abeanmination #{0}] Why did you eat bean {1} when bean {2} was perfectly available?", _moduleID, pos + 1, solution + 1);
						Module.HandleStrike();
						StartCoroutine(Strike());
					}
				}
				else
				{
					Debug.LogFormat("[Abeanmination #{0}] There was no valid bean, so you will not get a strike.", _moduleID);
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
					Solve();
				}
			}
			//Beans[pos].GetComponent<Renderer>().enabled = false;
			Beans[pos].transform.localScale = new Vector3(0f, 0f, 0f);
			return false;
		};
	}

	private void BeanHovered(int pos)
	{
		string[] colour = { " orange", " yellow", " green", " purple", " pink" };
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

		int[][] grid = new int[][] { new int[3], new int[3], new int[3] };
		List<int> types = new List<int> { };

		while (grid.Any(x => x.ToList().Contains(0)))
		{
			types.Add(Rnd.Range(0, 11));
			List<int> spots = new List<int> { };
			switch (types.Last())
			{
				case 3:
					for (int i = 0; i < 3; i++)
						if (grid[0][i] == 0 && grid[1][i] == 0 && grid[2][i] == 0)
							spots.Add(i);
					if (spots.Count() == 0)
						types.RemoveAt(types.Count() - 1);
					else
					{
						int pick = spots.PickRandom();
						for (int i = 0; i < 3; i++)
							grid[i][pick] = types.Count();
					}
					break;
				case 7:
					for (int i = 0; i < 8; i++)
						if (grid[(i / 2) % 2 + i / 4][i % 2] == 0 && grid[(i / 2) % 2 + 1 - i / 4][i % 2 + 1] == 0 && grid[(i / 2) % 2 + 1 - i / 4][i % 2] != grid[(i / 2) % 2 + i / 4][i % 2 + 1])
							spots.Add(i);
					if (spots.Count() == 0)
						types.RemoveAt(types.Count() - 1);
					else
					{
						int pick = spots.PickRandom();
						grid[(pick / 2) % 2 + pick / 4][pick % 2] = types.Count();
						grid[(pick / 2) % 2 + 1 - pick / 4][pick % 2 + 1] = types.Count();
					}
					break;
				case 9:
					for (int i = 0; i < 4; i++)
						if (grid[i / 2][i % 2] == 0 && grid[i / 2 + 1][i % 2] == 0 && grid[i / 2][i % 2 + 1] == 0 && grid[i / 2 + 1][i % 2 + 1] == 0)
							spots.Add(i);
					if (spots.Count() == 0)
						types.RemoveAt(types.Count() - 1);
					else
					{
						int pick = spots.PickRandom();
						for (int i = 0; i < 4; i++)
							grid[pick / 2 + i / 2][pick % 2 + i % 2] = types.Count();
					}
					break;
				default:
					for (int i = 0; i < 9; i++)
						if (grid[i / 3][i % 3] == 0)
							spots.Add(i);
					if (spots.Count() == 0)
						types.RemoveAt(types.Count() - 1);
					else
					{
						int pick = spots.PickRandom();
						grid[pick / 3][pick % 3] = types.Count();
					}
					break;
			}
		}
		Debug.LogFormat("[Abeanmination #{0}] Grid is {1} with beantypes {2}.", _moduleID, grid.Select(x => x.Join(",")).Join(";"), types.Join(","));

		bool ready = false;
		int[] solution = new int[3];
		while (!ready)
		{
			for (int i = 0; i < 9; i++)
			{
				switch (types[grid[i / 3][i % 3] - 1])
				{
					case 0:
						beanArray[i] = Rnd.Range(0, 6);
						break;
					case 1:
						beanArray[i] = Rnd.Range(0, 3);
						break;
					case 2:
					case 3:
						beanArray[i] = Rnd.Range(0, 2);
						break;
					case 4:
						beanArray[i] = Rnd.Range(0, 64);
						break;
					case 5:
						beanArray[i] = Rnd.Range(0, 3);
						break;
					case 6:
						beanArray[i] = Rnd.Range(0, 4);
						break;
					case 7:
						beanArray[i] = Rnd.Range(0, 9);
						for (int j = 0; j < 9; j++)
							if (grid[i / 3][i % 3] == grid[j / 3][j % 3])
								beanArray[j] = beanArray[i];
						break;
				}
			}
			ready = true;
		}
		Debug.LogFormat("[Abeanmination #{0}] Beans to eat in order are: {1}.", _moduleID, solution.Select(x => x + 1).Join(", "));
		for (int i = 0; i < 9; i++)
		{
			offset[i] = Rnd.Range(0f, 360f);
			switch (types[grid[i / 3][i % 3] - 1])
			{
				case 0:
				case 1:
				case 2:
				case 6:
					Jellybeans[i].GetComponent<MeshRenderer>().enabled = false;
					Kidneybeans[i].GetComponent<MeshRenderer>().enabled = false;
					Sqbeans[i].GetComponent<MeshRenderer>().enabled = false;
					Boozlebeans[i].transform.localScale = new Vector3(0, 0, 0);
					break;
				case 4:
					Beans[i].GetComponent<MeshRenderer>().enabled = false;
					Kidneybeans[i].GetComponent<MeshRenderer>().enabled = false;
					Sqbeans[i].GetComponent<MeshRenderer>().enabled = false;
					Boozlebeans[i].transform.localScale = new Vector3(0, 0, 0);
					break;
				case 5:
					Beans[i].GetComponent<MeshRenderer>().enabled = false;
					Jellybeans[i].GetComponent<MeshRenderer>().enabled = false;
					Sqbeans[i].GetComponent<MeshRenderer>().enabled = false;
					Boozlebeans[i].transform.localScale = new Vector3(0, 0, 0);
					break;
				case 8:
					Beans[i].GetComponent<MeshRenderer>().enabled = false;
					Jellybeans[i].GetComponent<MeshRenderer>().enabled = false;
					Kidneybeans[i].GetComponent<MeshRenderer>().enabled = false;
					Boozlebeans[i].transform.localScale = new Vector3(0, 0, 0);
					break;
				case 10:
					Beans[i].GetComponent<MeshRenderer>().enabled = false;
					Jellybeans[i].GetComponent<MeshRenderer>().enabled = false;
					Kidneybeans[i].GetComponent<MeshRenderer>().enabled = false;
					Sqbeans[i].GetComponent<MeshRenderer>().enabled = false;
					break;
				default:
					Jellybeans[i].transform.localScale = new Vector3(0, 0, 0);
					Kidneybeans[i].transform.localScale = new Vector3(0, 0, 0);
					Sqbeans[i].transform.localScale = new Vector3(0, 0, 0);
					Boozlebeans[i].transform.localScale = new Vector3(0, 0, 0);
					break;
			}
			switch (types[grid[i / 3][i % 3] - 1])
			{
				case 0:
					Beans[i].GetComponent<MeshRenderer>().material.color = new Color(colours[0][beanArray[i] % 3] / 255f, colours[1][beanArray[i] % 3] / 255f, colours[2][beanArray[i] % 3] / 255f);
					break;
				case 1:
					Beans[i].GetComponent<MeshRenderer>().material = Materials[0];
					break;
				case 2:
					Beans[i].GetComponent<MeshRenderer>().material = Materials[2];
					Beans[i].GetComponent<MeshRenderer>().material.color = new Color(rottencolours[0][beanArray[i]] / 255f, rottencolours[1][beanArray[i]] / 255f, rottencolours[2][beanArray[i]] / 255f);
					break;
				case 3:
					Longbeanlets[i].transform.localScale *= beanArray[i];
					break;
				case 4:
					Jellybeans[i].GetComponent<MeshRenderer>().material.color = new Color((beanArray[i] / 16) / 3f, ((beanArray[i] / 4) % 4) / 3f, (beanArray[i] % 4) / 3f);
					break;
				case 5:
					Kidneybeans[i].GetComponent<MeshRenderer>().material.color = new Color(kidneycolours[0][beanArray[i]] / 255f, kidneycolours[1][beanArray[i]] / 255f, kidneycolours[2][beanArray[i]] / 255f);
					break;
				case 6:
					Beans[i].GetComponent<MeshRenderer>().material = Materials[3];
					Beans[i].GetComponent<MeshRenderer>().material.color = new Color(chillicolours[0][beanArray[i]] / 255f, chillicolours[1][beanArray[i]] / 255f, chillicolours[2][beanArray[i]] / 255f);
					break;
				case 7:
					//Beans[i].GetComponent<MeshRenderer>().material = Materials[3];
					//Beans[i].GetComponent<MeshRenderer>().material.color = new Color(chillicolours[0][beanArray[i]] / 255f, chillicolours[1][beanArray[i]] / 255f, chillicolours[2][beanArray[i]] / 255f);
					break;
			}
		}
		for (int i = 0; i < 9; i++)
		{
			offset[i] = Rnd.Range(0f, 360f);
			switch (types[grid[i / 3][i % 3] - 1])
			{
				case 3:
					Beans[i].GetComponent<MeshRenderer>().enabled = false;
					if (i / 3 != 2 && i % 3 != 2)
						BeanSprouts[(i / 3 * 2 + i % 3) * 2].transform.localScale = new Vector3(0, 0, 0);
					else if (i / 3 != 0 && i % 3 != 0)
						BeanSprouts[(i / 3 * 2 + i % 3 - 3) * 2].transform.localScale = new Vector3(0, 0, 0);
					if (i / 3 != 2 && i % 3 != 0)
						BeanSprouts[(i / 3 * 2 + (i % 3 - 1)) * 2 + 1].transform.localScale = new Vector3(0, 0, 0);
					else if (i / 3 != 0 && i % 3 != 2)
						BeanSprouts[((i / 3 - 1) * 2 + i % 3) * 2 + 1].transform.localScale = new Vector3(0, 0, 0);
					if (i / 3 != 2 && i % 3 != 2)
						BigBeans[(i / 3) * 2 + i % 3].transform.localScale = new Vector3(0, 0, 0);
					else if (i / 3 != 0 && i % 3 != 0)
						BigBeans[(i / 3) * 2 + i % 3 - 3].transform.localScale = new Vector3(0, 0, 0);
					break;
				case 7:
					Beans[i].transform.localScale = new Vector3(0, 0, 0);
					LongBeans[i % 3].transform.localScale = new Vector3(0, 0, 0);
					Longbeanlets[i % 3 * 3 + i / 3].transform.localScale = new Vector3(0, 0, 0);
					if (i / 3 != 2 && i % 3 != 2)
						BigBeans[(i / 3) * 2 + i % 3].transform.localScale = new Vector3(0, 0, 0);
					else if (i / 3 != 0 && i % 3 != 0)
						BigBeans[(i / 3) * 2 + i % 3 - 3].transform.localScale = new Vector3(0, 0, 0);
					break;
				case 9:
					Beans[i].transform.localScale = new Vector3(0, 0, 0);
					LongBeans[i % 3].transform.localScale = new Vector3(0, 0, 0);
					Longbeanlets[i % 3 * 3 + i / 3].transform.localScale = new Vector3(0, 0, 0);
					if (i / 3 != 2 && i % 3 != 2)
						BeanSprouts[(i / 3 * 2 + i % 3) * 2].transform.localScale = new Vector3(0, 0, 0);
					else if (i / 3 != 0 && i % 3 != 0)
						BeanSprouts[(i / 3 * 2 + i % 3 - 3) * 2].transform.localScale = new Vector3(0, 0, 0);
					if (i / 3 != 2 && i % 3 != 0)
						BeanSprouts[(i / 3 * 2 + (i % 3 - 1)) * 2 + 1].transform.localScale = new Vector3(0, 0, 0);
					else if (i / 3 != 0 && i % 3 != 2)
						BeanSprouts[((i / 3 - 1) * 2 + i % 3) * 2 + 1].transform.localScale = new Vector3(0, 0, 0);
					break;
				default:
					LongBeans[i % 3].transform.localScale = new Vector3(0, 0, 0);
					Longbeanlets[i % 3 * 3 + i / 3].transform.localScale = new Vector3(0, 0, 0);
					if (i / 3 != 2 && i % 3 != 2)
						BeanSprouts[(i / 3 * 2 + i % 3) * 2].transform.localScale = new Vector3(0, 0, 0);
					else if (i / 3 != 0 && i % 3 != 0)
						BeanSprouts[(i / 3 * 2 + i % 3 - 3) * 2].transform.localScale = new Vector3(0, 0, 0);
					if (i / 3 != 2 && i % 3 != 0)
						BeanSprouts[(i / 3 * 2 + (i % 3 - 1)) * 2 + 1].transform.localScale = new Vector3(0, 0, 0);
					else if (i / 3 != 0 && i % 3 != 2)
						BeanSprouts[((i / 3 - 1) * 2 + i % 3) * 2 + 1].transform.localScale = new Vector3(0, 0, 0);
					if (i / 3 != 2 && i % 3 != 2)
						BigBeans[(i / 3) * 2 + i % 3].transform.localScale = new Vector3(0, 0, 0);
					else if (i / 3 != 0 && i % 3 != 0)
						BigBeans[(i / 3) * 2 + i % 3 - 3].transform.localScale = new Vector3(0, 0, 0);
					break;
			}
		}
		//StartCoroutine(Wobble());
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
