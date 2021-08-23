using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class beanboozledAgainScript : MonoBehaviour {

    public KMAudio Audio;
    public AudioClip[] sounds;
    public KMBombInfo BombInfo;
    public KMSelectable[] Beans;
    public GameObject Text;
    public TextMesh[] Display;
    public TextMesh[] BeanTexts;
    public MeshRenderer[] BeanLights;
    public GameObject Statuslight;
    public KMBombModule Module;

    private float[] offset = new float[8];
    private float[] timeoffset = new float[8];
    private float[] wobbliness = new float[8];
    private float[] rotational = new float[8];
    private int[][] beanArray = new int[8][];
    private int[] numkey = new int[3];
    private int[][] setnums = new int[][] { new int[] { }, new int[] { }, new int[] { }, new int[] { } };
    private readonly int[][] colours = { new int[] { 0, 0, 0 }, new int[] { 1, 1, 0 }, new int[] { 2, 1, 0 }, new int[] { 1, 2, 0 }, new int[] { 2, 2, 0 }, new int[] { 3, 1, 0 }, new int[] { 1, 3, 0 }, new int[] { 3, 2, 0 }, new int[] { 2, 3, 0 }, new int[] { 3, 3, 0 }, new int[] { 2, 2, 1 }, new int[] { 3, 2, 1 }, new int[] { 2, 3, 1 }, new int[] { 3, 3, 1 }, new int[] { 3, 3, 2 }, new int[] { 3, 3, 3 } };
    private readonly string[] words = { "BEANBOOZLED", "BEANEDAGAIN", "BAMBOOZLING", "TOOCOLOURFUL", "BEANINGS", "HYPERBEANS", "ULTRABEANS", "COOLBEANS", "ROTTENBEANS", "JELLYBEANS", "LONGBEANS", "BEANBEANBEAN", "NOTGOODBEAN", "SAUCEDBEANS", "BAKEDBEANS", "BEANOVERLOAD", "BEANCIPHER", "YOUCANTBEATBEANS", "BURNEDBEAN", "KILLERBEAN", "INEDIBLE", "SURELYNOTCORN", "BEANKRUPT", "SUPERMARKET", "GROCERIES", "ROWANATKINSON", "MEANDTHEBOYS", "AT3AMLOOKING", "FORBEANS", "DANSBEANS", "POGBEANS", "BEANSANITY" };
    private int wordindex;
    private int[] Initvalues = new int[8];
    private int[] BeanFmem = { 0, -1, -1, -1 };
    private int[] BeanFend = new int[8];
    private int[] Setnums = new int[8];
    private int[] Totalnums = new int[8];
    private int[] ColValues = new int[8];
    private int[] GoodPress = new int[2];
    private int[] PressIndices = new int[4];
    private bool[] correct = new bool[5];
    private bool[] almost = new bool[5];
    private int presscount = 0;

    static int _moduleIdCounter = 1;
    int _moduleID = 0;

    private KMSelectable.OnInteractHandler BeanPressed(int pos)
    {
        return delegate
        {
            Audio.PlaySoundAtTransform("Monch", Module.transform);
            if (presscount != 5)
            {
                if (presscount == 0)
                    for (int i = 0; i < 5; i++)
                        BeanLights[i].material.color = new Color(0, 0, 0);
                if (pos == GoodPress[0] && ((((int)BombInfo.GetTime() % (Setnums[pos] + ((wordindex / 8) * 10 + wordindex % 8 + 11)) - 1) % 9 + 1 == GoodPress[1] && presscount < 4) || ((int)BombInfo.GetTime() % 2 == GoodPress[1] && presscount == 4)))
                {
                    correct[presscount] = true;
                    Debug.LogFormat("[Beanboozled Again #{0}] You pressed bean {1} at {2} seconds which is correct.", _moduleID, pos + 1, (int)BombInfo.GetTime());
                }
                else if (pos == GoodPress[0])
                {
                    almost[presscount] = true;
                    Debug.LogFormat("[Beanboozled Again #{0}] You pressed bean {1} at {2} seconds which is almost correct.", _moduleID, pos + 1, (int)BombInfo.GetTime());
                }
                else
                    Debug.LogFormat("[Beanboozled Again #{0}] You pressed bean {1} at {2} seconds which is incorrect.", _moduleID, pos + 1, (int)BombInfo.GetTime());

                BeanLights[presscount].material.color = new Color(1, 1, 1);
                presscount++;
                if (presscount == 5)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        if (correct[i])
                            BeanLights[i].material.color = new Color(0.33f, 1, 0);
                        else if (almost[i])
                            BeanLights[i].material.color = new Color(1, 1, 0);
                        else
                            BeanLights[i].material.color = new Color(1, 0.33f, 0);
                    }
                    if (correct.Count(x => x) == 5)
                    {
                        Module.HandlePass();
                        Solve();
                        for (int i = 0; i < Beans.Length; i++)
                        {
                            Beans[i].GetComponent<MeshRenderer>().material.color = new Color(0.33f, 1, 0);
                            BeanTexts[2 * i].color = new Color(0.33f, 1, 0);
                            BeanTexts[2 * i + 1].color = new Color(0.33f, 1, 0);
                        }
                        for (int i = 0; i < 2; i++)
                        {
                            Display[i].text = "XYPFWYPI";
                            Display[i].color = new Color(0.33f, 1, 0);
                        }
                    }
                    else if (correct.Count(x => x) + almost.Count(x => x) == 5)
                    {
                        presscount = 0;
                        int[] randomnums = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
                        for (int i = 0; i < 3; i++)
                            beanArray[pos][i] = randomnums.Where(x => !beanArray[pos].Take(i).Contains(x)).ToArray()[Rnd.Range(0, randomnums.Where(x => !beanArray[pos].Take(i).Contains(x)).ToArray().Length)];
                        for (int i = 0; i < 2; i++)
                            BeanTexts[pos * 2 + i].text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123"[Rnd.Range(0, 30)].ToString();
                        Beans[pos].GetComponent<MeshRenderer>().material.color = new Color(colours[beanArray[pos][0]][0] / 3f, colours[beanArray[pos][0]][1] / 3f, colours[beanArray[pos][0]][2] / 3f);
                        BeanTexts[2 * pos].color = new Color(colours[beanArray[pos][1]][0] / 3f, colours[beanArray[pos][1]][1] / 3f, colours[beanArray[pos][1]][2] / 3f);
                        BeanTexts[2 * pos + 1].color = new Color(colours[beanArray[pos][2]][0] / 3f, colours[beanArray[pos][2]][1] / 3f, colours[beanArray[pos][2]][2] / 3f);
                        CalcBeans();
                    }
                    else
                    {
                        Debug.LogFormat("[Beanboozled Again #{0}] Correct: {1}. Almost: {2}.", _moduleID, correct.Join(", "), almost.Join(", "));
                        correct = new bool[5];
                        almost = new bool[5];
                        presscount = 0;
                        Module.HandleStrike();
                        StartCoroutine(Strike());
                        wordindex = Rnd.Range(0, words.Length);
                        for (int i = 0; i < 8; i++)
                        {
                            offset[i] = Rnd.Range(0f, 360f);
                            timeoffset[i] = Rnd.Range(0f, 360f);
                            wobbliness[i] = Rnd.Range(0f, 30f);
                            rotational[i] = Rnd.Range(-10f, 10f);
                            beanArray[i] = new int[] { Rnd.Range(0, 16) };
                            int[] randomnums = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
                            beanArray[i] = beanArray[i].Concat(new int[] { randomnums.Where(x => x != beanArray[i][0]).ToArray()[Rnd.Range(0, 15)] }).ToArray();
                            beanArray[i] = beanArray[i].Concat(new int[] { randomnums.Where(x => !beanArray[i].Contains(x)).ToArray()[Rnd.Range(0, 14)] }).ToArray();
                            beanArray[i] = beanArray[i].Concat(new int[] { Rnd.Range(1, 16), Rnd.Range(1, 16) }).ToArray();
                            for (int j = 0; j < 2; j++)
                                setnums[j] = setnums[j].Concat(new int[] { Rnd.Range(0, 5) }).ToArray();
                        }
                        while (numkey.Count(x => x == 0) == 3)
                            for (int i = 0; i < 3; i++)
                                numkey[i] = Rnd.Range(0, 3);
                        string encryption = EncryptDisplay(words[wordindex], numkey);
                        Debug.LogFormat("[Beanboozled Again #{0}] Number key {1} and word {2} resulted in {3}. Display value is {4}.", _moduleID, numkey.Join(""), words[wordindex], encryption, (wordindex / 8) * 10 + wordindex % 8 + 11);
                        Display[0].text = "";
                        Display[1].text = "";
                        for (int i = 0; i < 8; i++)
                        {
                            Beans[i].GetComponent<MeshRenderer>().material.color = new Color(colours[beanArray[i][0]][0] / 3f, colours[beanArray[i][0]][1] / 3f, colours[beanArray[i][0]][2] / 3f);
                            BeanTexts[2 * i].color = new Color(colours[beanArray[i][1]][0] / 3f, colours[beanArray[i][1]][1] / 3f, colours[beanArray[i][1]][2] / 3f);
                            BeanTexts[2 * i + 1].color = new Color(colours[beanArray[i][2]][0] / 3f, colours[beanArray[i][2]][1] / 3f, colours[beanArray[i][2]][2] / 3f);
                            BeanTexts[2 * i].text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123"[Rnd.Range(0, 30)].ToString();
                            BeanTexts[2 * i + 1].text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123"[Rnd.Range(0, 30)].ToString();
                            Display[0].text += "<color='#" + colours[beanArray[i][3]].Select(x => (new string[] { "00", "55", "aa", "ff" })[x]).Join("") + "'>" + CharEncrypt(encryption[i], new int[] { setnums[0][i], 0 })[0] + "</color>";
                            Display[1].text = "<color='#" + colours[beanArray[i][4]].Select(x => (new string[] { "00", "55", "aa", "ff" })[x]).Join("") + "'>" + CharEncrypt(encryption[i], new int[] { 0, setnums[1][i] })[1] + "</color>" + Display[1].text;
                        }
                        CalcBeans();
                    }
                }
                else
                {
                    int[] randomnums = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
                    for (int i = 0; i < 3; i++)
                        beanArray[pos][i] = randomnums.Where(x => !beanArray[pos].Take(i).Contains(x)).ToArray()[Rnd.Range(0, randomnums.Where(x => !beanArray[pos].Take(i).Contains(x)).ToArray().Length)];
                    for (int i = 0; i < 2; i++)
                        BeanTexts[pos * 2 + i].text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123"[Rnd.Range(0, 30)].ToString();
                    Beans[pos].GetComponent<MeshRenderer>().material.color = new Color(colours[beanArray[pos][0]][0] / 3f, colours[beanArray[pos][0]][1] / 3f, colours[beanArray[pos][0]][2] / 3f);
                    BeanTexts[2 * pos].color = new Color(colours[beanArray[pos][1]][0] / 3f, colours[beanArray[pos][1]][1] / 3f, colours[beanArray[pos][1]][2] / 3f);
                    BeanTexts[2 * pos + 1].color = new Color(colours[beanArray[pos][2]][0] / 3f, colours[beanArray[pos][2]][1] / 3f, colours[beanArray[pos][2]][2] / 3f);
                    CalcBeans();
                }
            }
            else
                Beans[pos].transform.localScale = new Vector3(0f, 0f, 0f);
            return false;
        };
    }

    private void BeanHovered(int pos)
    {
        string[] colour = { "Bk", "Ol", "Bn", "Lf", "Yk", "Ac", "Li", "Gd", "Pe", "Yl", "Bd", "Be", "Mi", "Le", "We", "Wt" };
        if (presscount != 5)
            Text.GetComponent<TextMesh>().text = (pos + 1) + "-" + colour[beanArray[pos][0]] + "," + colour[beanArray[pos][1]] + colour[beanArray[pos][2]] + "," + colour[beanArray[pos][3]] + colour[beanArray[pos][4]];
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
        for (int i = 0; i < 5; i++)
            BeanLights[i].material.color = new Color(0, 0, 0);
        wordindex = Rnd.Range(0, words.Length);
        for (int i = 0; i < 8; i++)
        {
            offset[i] = Rnd.Range(0f, 360f);
            timeoffset[i] = Rnd.Range(0f, 360f);
            wobbliness[i] = Rnd.Range(0f, 30f);
            while (rotational[i] < 0.5f && rotational[i] > -0.5f)
                rotational[i] = Rnd.Range(-10f, 10f);
            beanArray[i] = new int[] { Rnd.Range(0, 16) };
            int[] randomnums = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            beanArray[i] = beanArray[i].Concat(new int[] { randomnums.Where(x => x != beanArray[i][0]).ToArray()[Rnd.Range(0, 15)] }).ToArray();
            beanArray[i] = beanArray[i].Concat(new int[] { randomnums.Where(x => !beanArray[i].Contains(x)).ToArray()[Rnd.Range(0, 14)] }).ToArray();
            beanArray[i] = beanArray[i].Concat(new int[] { Rnd.Range(1, 16), Rnd.Range(1, 16) }).ToArray();
            for (int j = 0; j < 2; j++)
                setnums[j] = setnums[j].Concat(new int[] { Rnd.Range(0, 5) }).ToArray();
        }
        while (numkey.Count(x => x == 0) == 3)
            for (int i = 0; i < 3; i++)
                numkey[i] = Rnd.Range(0, 3);
        string encryption = EncryptDisplay(words[wordindex], numkey);
        Debug.LogFormat("[Beanboozled Again #{0}] Number key {1} and word {2} resulted in {3}. Display value is {4}.", _moduleID, numkey.Join(""), words[wordindex], encryption, (wordindex / 8) * 10 + wordindex % 8 + 11);
        Display[0].text = "";
        Display[1].text = "";
        for (int i = 0; i < 8; i++)
        {
            Beans[i].GetComponent<MeshRenderer>().material.color = new Color(colours[beanArray[i][0]][0] / 3f, colours[beanArray[i][0]][1] / 3f, colours[beanArray[i][0]][2] / 3f);
            BeanTexts[2 * i].color = new Color(colours[beanArray[i][1]][0] / 3f, colours[beanArray[i][1]][1] / 3f, colours[beanArray[i][1]][2] / 3f);
            BeanTexts[2 * i + 1].color = new Color(colours[beanArray[i][2]][0] / 3f, colours[beanArray[i][2]][1] / 3f, colours[beanArray[i][2]][2] / 3f);
            BeanTexts[2 * i].text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123"[Rnd.Range(0, 30)].ToString();
            BeanTexts[2 * i + 1].text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123"[Rnd.Range(0, 30)].ToString();
            Display[0].text += "<color='#" + colours[beanArray[i][3]].Select(x => (new string[] { "00", "55", "aa", "ff" })[x]).Join("") + "'>" + CharEncrypt(encryption[i], new int[] { setnums[0][i], 0 })[0] + "</color>";
            Display[1].text = "<color='#" + colours[beanArray[i][4]].Select(x => (new string[] { "00", "55", "aa", "ff" })[x]).Join("") + "'>" + CharEncrypt(encryption[i], new int[] { 0, setnums[1][i] })[1] + "</color>" + Display[1].text;
        }
        CalcBeans();
        StartCoroutine(Wobble());
    }

    private void CalcBeans()
    {
        string chars36 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        for (int i = 0; i < 8; i++)
        {
            int[] base36 = new int[2];
            for (int j = 0; j < 36; j++)
            {
                if (chars36[j] == CharDecrypt(new char[] { BeanTexts[2 * i].text[0], BeanTexts[2 * i + 1].text[0] }))
                    base36[0] = j;
                if (chars36[j] == CharDecrypt(new char[] { BeanTexts[2 * i + 1].text[0], BeanTexts[2 * i].text[0] }))
                    base36[1] = j;
            }
            Initvalues[i] = (base36[0] + base36[1]) % 36;
        }
        Debug.LogFormat("[Beanboozled Again #{0}] I values in order are {1}.", _moduleID, Initvalues.Select(x => chars36[x]).Join(""));
        string[] interpretant = { "<>+-[],.←→↑↓*∴∵", ">,-↓↑∵]+→←*.∴<[", "-→*←↑.<+↓>]∴[,∵", "→↑↓[>∴←],*+<∵-.", "∵.←+∴-↓*→>[,↑<]", "]>←[→↓↑*∵.+∴-,<", ",↑>*∴→<[↓].-+∵←", "←→-↑+>∴∵*.↓<][," };
        BeanFend = new int[8];
        string command = "";
        for (int i = 0; i < 8; i++)
            command += interpretant[i][beanArray[i][3] - 1];
        for (int i = 0; i < 8; i++)
            command += interpretant[i][beanArray[i][4] - 1];
        BeanFmem = new int[] { wordindex % 8, -1, -1, -1 };
        BeanFend = BeanF(Initvalues, command, 1);
        Debug.LogFormat("[Beanboozled Again #{0}] Used command is '{1}', resulting in {2}.", _moduleID, command, BeanFend.Select(x => chars36[x]).Join(""));
        for (int i = 0; i < 8; i++)
            Setnums[i] = int.Parse((setnums[0][i] * 10 + setnums[1][i] + 11).ToString() + (SetDecrypt(BeanTexts[2 * i].text[0]) + SetDecrypt(BeanTexts[2 * i + 1].text[0])).ToString());
        Debug.LogFormat("[Beanboozled Again #{0}] Raw set values before modulo are {1}.", _moduleID, Setnums.Join(", "));
        for (int i = 0; i < 8; i++)
            Setnums[i] %= BeanFend[i] + 1;
        Debug.LogFormat("[Beanboozled Again #{0}] Set values after modulo are {1}.", _moduleID, Setnums.Join(", "));
        for (int i = 0; i < 8; i++)
            Totalnums[i] = Setnums[i] * (numkey[0] * 100 + numkey[1] * 10 + numkey[2]) + BeanFend[i];
        Debug.LogFormat("[Beanboozled Again #{0}] Total values are {1}.", _moduleID, Totalnums.Join(", "));
        for (int i = 0; i < 8; i++)
            ColValues[i] = beanArray[i][0] * 241 + beanArray[i][1] * beanArray[i][2];
        Debug.LogFormat("[Beanboozled Again #{0}] Colour values are {1}.", _moduleID, ColValues.Join(", "));
        List<int> tempvalues = Totalnums.ToList();
        int[] order = { };
        while (tempvalues.ToArray().Length > 0)
        {
            for (int i = 0; i < 8; i++)
                if (Totalnums[i] == tempvalues.Min())
                    order = order.Concat(new int[] { i }).ToArray();
            int min = tempvalues.Min();
            for (int i = 0; i < tempvalues.ToArray().Length; i++)
                if (tempvalues.ToArray()[i] == min)
                {
                    tempvalues.RemoveAt(i);
                    i--;
                }
        }
        Debug.LogFormat("[Beanboozled Again #{0}] Bean indices from lowest to highest are {1}.", _moduleID, order.Select(x => x + 1).Join(", "));
        GoodPress[0] = order[presscount + 2];
        if (presscount < 4)
        {
            GoodPress[1] = (ColValues[GoodPress[0]] - 1) % 9 + 1;
            PressIndices[presscount] = GoodPress[0];
            Debug.LogFormat("[Beanboozled Again #{0}] Pressed {1} beans so far, making the bean to press bean {2}. Formula becomes: DR(t % {3}) = {4}.", _moduleID, presscount, GoodPress[0] + 1, Setnums[GoodPress[0]] + ((wordindex / 8) * 10 + wordindex % 8 + 11), GoodPress[1]);
        }
        else
        {
            GoodPress[1] = rotational[PressIndices[wordindex / 8]] < 0 ? 0 : 1;
            Debug.LogFormat("[Beanboozled Again #{0}] Pressed {1} beans so far, making the bean to press bean {2}. Formula becomes: t % 2 = {3}.", _moduleID, presscount, GoodPress[0] + 1, GoodPress[1]);
        }
    }
    private IEnumerator Wobble()
    {
        float t = 0;
        while (true)
        {
            for (int i = 0; i < 8; i++)
                Beans[i].transform.localEulerAngles = new Vector3(-90f, Mathf.Sin(t + timeoffset[i]) * wobbliness[i] + rotational[i] * t + offset[i], 0f);
            yield return null;
            t += Time.deltaTime * 10f;
        }
    }

    private string EncryptDisplay(string word, int[] offset)
    {
        //shortening
        while (word.Length > 8)
        {
            int rnd = Rnd.Range(0, word.Length);
            string newword = "";
            for (int i = 0; i < word.Length; i++)
                if (!(rnd == i))
                    newword += word[i];
            word = newword;
        }
        //cycling
        {
            int rnd = Rnd.Range(0, 8);
            string newword = "";
            for (int i = 0; i < 8; i++)
                newword += word[(i + rnd) % 8];
            word = newword;
        }
        //ciphering
        string result = "";
        for (int i = 0; i < 8; i++)
            result += Encipher(word[i]);
        return result;
    }

    private char[] CharEncrypt(char letter, int[] keyvalues)
    {
        string key = "S50J8XOWZ1HFVMQ4EL3DTC6UGN2RAYP9KIB7";
        int x = 0;
        int y = 0;
        for (int i = 0; i < 36; i++)
            if (letter == key[i])
            {
                x = i % 6;
                y = i / 6;
                string topkey = "BFQNXLOJUHDTAIGM0ZC1KEY32WSVPR";
                string bottomkey = "LOQXBAMZRSWI0GJ2YHDT1KEC3NPFUV";
                return new char[] { topkey[x + 6 * keyvalues[0]], bottomkey[y + 6 * keyvalues[1]] };
            }
        return "".ToArray();
    }

    private int[] BeanF(int[] storage, string command, int bean)
    {
        //beanfmem {pivot, mem, X, Y}
        string chars36 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        for (int i = 0; i < command.Length; i++)
        {
            switch (command[i])
            {
                case '>':
                    BeanFmem[0] = (BeanFmem[0] + 1) % 8;
                    break;
                case '<':
                    BeanFmem[0] = (BeanFmem[0] + 7) % 8;
                    break;
                case '+':
                    storage[BeanFmem[0]] = (storage[BeanFmem[0]] + 1) % 36;
                    break;
                case '-':
                    storage[BeanFmem[0]] = (storage[BeanFmem[0]] + 35) % 36;
                    break;
                case '[':
                    bool steady = true;
                    int loops = 0;
                    string cmd = "";
                    for (int j = i + 1; j < command.Length && steady; j++)
                    {
                        if (command[j] == ']')
                        {
                            if (loops == 0)
                            {
                                steady = false;
                                for (int k = 0; k < bean; k++)
                                    storage = BeanF(storage, cmd, bean);
                            }
                            else
                            {
                                loops--;
                                cmd += command[j];
                            }
                        }
                        else
                        {
                            cmd += command[j];
                            if (command[j] == '[')
                                loops++;
                        }
                    }
                    break;
                case ',':
                    BeanFmem[1] = storage[BeanFmem[0]];
                    break;
                case '.':
                    if (BeanFmem[1] != -1)
                        storage[BeanFmem[0]] = BeanFmem[1];
                    break;
                case '←':
                    {
                        if (storage[(BeanFmem[0] + 7) % 8] < storage[BeanFmem[0]])
                        BeanFmem[0] = (BeanFmem[0] + 7) % 8;
                    }
                    break;
                case '→':
                    {
                        if (storage[(BeanFmem[0] + 1) % 8] > storage[BeanFmem[0]])
                            BeanFmem[0] = (BeanFmem[0] + 1) % 8;
                    }
                    break;
                case '↑':
                    if ((storage[BeanFmem[0]] + storage[(BeanFmem[0] + 4) % 8]) % 2 == 0)
                        storage[BeanFmem[0]] = (storage[BeanFmem[0]] + 1) % 36;
                    break;
                case '↓':
                    if ((storage[BeanFmem[0]] + storage[(BeanFmem[0] + 4) % 8]) % 2 == 1)
                        storage[BeanFmem[0]] = (storage[BeanFmem[0]] + 35) % 36;
                    break;
                case '*':
                    if (BeanFmem[2] == -1)
                        BeanFmem[2] = BeanFmem[0];
                    else
                    {
                        BeanFmem[3] = BeanFmem[0];
                        int backup = storage[BeanFmem[2]];
                        storage[BeanFmem[2]] = storage[BeanFmem[3]];
                        storage[BeanFmem[3]] = backup;
                        BeanFmem[2] = -1;
                        BeanFmem[3] = -1;
                    }
                    break;
                case '∴':
                    {
                        char ciphered = Encipher(chars36[storage[BeanFmem[0]]]);
                        for (int k = 0; k < 36; k++)
                            if (chars36[k] == ciphered)
                                storage[BeanFmem[0]] = k;
                    }
                    break;
                case '∵':
                    {
                        char ciphered = Decipher(chars36[storage[BeanFmem[0]]]);
                        for (int k = 0; k < 36; k++)
                            if (chars36[k] == ciphered)
                                storage[BeanFmem[0]] = k;
                    }
                    break;
                default:
                    break;
            }
        }
        return storage;
    }

    private char CharDecrypt(char[] letter)
    {
        string key = "S50J8XOWZ1HFVMQ4EL3DTC6UGN2RAYP9KIB7";
        string topkey = "BFQNXLOJUHDTAIGM0ZC1KEY32WSVPR";
        string bottomkey = "LOQXBAMZRSWI0GJ2YHDT1KEC3NPFUV";
        int x = 0;
        int y = 0;
        for (int i = 0; i < 30; i++)
        {
            if (topkey[i] == letter[0])
                x = i % 6;
            if (bottomkey[i] == letter[1])
                y = i % 6;
        }
        return key[x + y * 6];
    }

    private int SetDecrypt(char letter)
    {
        string topkey = "BFQNXLOJUHDTAIGM0ZC1KEY32WSVPR";
        string bottomkey = "LOQXBAMZRSWI0GJ2YHDT1KEC3NPFUV";
        int values = 0;
        for (int i = 0; i < 30; i++)
        {
            if (topkey[i] == letter)
                values += (i / 6) + 1;
            if (bottomkey[i] == letter)
                values += (i / 6) + 1;
        }
        return values;
    }

    private char Encipher(char letter)
    {
        string basekey = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string[] truekey = { "OFGNETV10Z5Y32S7DXBKLJ9C8QUH4APW6RIM", "HCP4FTVNRZ58SU73QYOGD29EXLJB1AKW6I0M", "0UFE9Y6Q8CBR41ZP75XJKLNSO2GAMIDVWTH3" };
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < numkey[i]; j++)
                for (int k = 0; k < 36; k++)
                    if (basekey[k] == letter)
                    {
                        letter = truekey[i][k];
                        k = 36;
                    }
        return letter;
    }

    private char Decipher(char letter)
    {
        string basekey = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string[] truekey = { "OFGNETV10Z5Y32S7DXBKLJ9C8QUH4APW6RIM", "HCP4FTVNRZ58SU73QYOGD29EXLJB1AKW6I0M", "0UFE9Y6Q8CBR41ZP75XJKLNSO2GAMIDVWTH3" };
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < numkey[2 - i]; j++)
                for (int k = 0; k < 36; k++)
                    if (truekey[2 - i][k] == letter)
                    {
                        letter = basekey[k];
                        k = 36;
                    }
        return letter;
    }

    private IEnumerator Strike()
    {
        Statuslight.GetComponent<MeshRenderer>().material.color = new Color(1f, 0.33f, 0f);
        yield return new WaitForSeconds(0.5f);
        Statuslight.GetComponent<MeshRenderer>().material.color = new Color(0.67f, 0.33f, 0f);
    }

    private void Solve()
    {
        Statuslight.GetComponent<MeshRenderer>().material.color = new Color(0.33f, 1f, 0f);
    }

#pragma warning disable 414
    private string TwitchHelpMessage = "'!{0}' reset to reset the inputs, '!{0} cycle' to cycle through the beans, '!{0} 1 at 69420' to eat a bean in reading order (indexed at 1) when the total amount of seconds on the timer is 69420.";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        yield return null;
        command = command.ToLowerInvariant();
        if (command == "reset")
        {
            correct = new bool[5];
            almost = new bool[5];
            presscount = 0;
            for (int i = 0; i < BeanLights.Length; i++)
                BeanLights[i].GetComponent<MeshRenderer>().material.color = new Color(0f, 0f, 0f);
            Debug.LogFormat("[Beanboozled Again #{0}] Resetted inputs.", _moduleID);
            CalcBeans();
        }
        else if (command == "cycle")
            for (int i = 0; i < 8; i++)
            {
                while (i < 8 && Beans[i].transform.localScale.x < 0.01f)
                    i++;
                if (i != 8)
                {
                    Beans[i].OnHighlight();
                    yield return new WaitForSeconds(0.9f);
                    Beans[i].OnHighlightEnded();
                    yield return new WaitForSeconds(0.1f);
                }
            }
        else
        {
            string validCommands = "12345678";
            string[] cmds = command.Split(' ');
            int a;
            if (!validCommands.Contains(cmds[0][0]) || cmds[0].Length != 1 || cmds[1] != "at" || !int.TryParse(cmds[2], out a) || cmds.Length != 3)
            {
                yield return "sendtochaterror Invalid command.";
                yield break;
            }
            yield return "strike";
            yield return "solve";
            while ((int)BombInfo.GetTime() != int.Parse(cmds[2]))
                yield return "trycancel Your button press for Beanboozled Again is cancelled.";
            for (int i = 0; i < validCommands.Length; i++)
                if (validCommands[i] == cmds[0][0])
                    Beans[i].OnInteract();
        }
        yield return null;
    }
    
    IEnumerator TwitchHandleForcedSolve()
    {
        yield return true;
        if (correct.Count(x => x) + almost.Count(x => x) != presscount)
        {
            correct = new bool[5];
            almost = new bool[5];
            presscount = 0;
            for (int i = 0; i < BeanLights.Length; i++)
                BeanLights[i].GetComponent<MeshRenderer>().material.color = new Color(0f, 0f, 0f);
            CalcBeans();
        }
        while (presscount != 5)
        {
            if (correct.Count(x => x) == presscount)
                while (!((((int)BombInfo.GetTime() % (Setnums[GoodPress[0]] + ((wordindex / 8) * 10 + wordindex % 8 + 11)) - 1) % 9 + 1 == GoodPress[1] && presscount < 4) || ((int)BombInfo.GetTime() % 2 == GoodPress[1] && presscount == 4)))
                    yield return true;
            Beans[GoodPress[0]].OnInteract();
            yield return true;
        }
    }
}
