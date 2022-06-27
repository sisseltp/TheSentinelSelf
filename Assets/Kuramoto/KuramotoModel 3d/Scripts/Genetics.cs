using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Genetics
{

    public struct GenVel
    {

        public GenVel(Vector3[] vels, float fit = 0)
        {
            Vels = vels;
            fitness = fit;
        }

        public Vector3[] BlendAttributes(Vector3[] otherVels)
        {
            Vector3[] newVels = new Vector3[Vels.Length];
            for (int i = 0; i < newVels.Length; i++)
            {

                float rand = UnityEngine.Random.value;

                if (rand < 0.33f)
                {
                    newVels[i] = Vels[i];
                }
                else if (rand < 0.66f)
                {
                    newVels[i] = otherVels[i];
                }
                else
                {
                    newVels[i] = UnityEngine.Random.insideUnitSphere;
                }

            }



            return newVels;
        }

        public Vector3[] Vels;
        public float fitness;

    }
    // struct to holg gene kurmto data
    public struct GenKurmto
    {
        public float[] Settings;
        public float fitness;
        // constructor 
        public GenKurmto(float speed, float noiseScl, float coupling, float couplingRange, float attractionScl, float fit)
        {
            Settings = new float[5];
            Settings[0] = speed;
            Settings[1] = noiseScl;
            Settings[2] = coupling;
            Settings[3] = couplingRange;
            Settings[4] = attractionScl;
            fitness = fit;
        }

        public float[] BlendAttributes(float[] otherSettings)
        {
            float[] newSetting = new float[Settings.Length];
            for (int i = 0; i < newSetting.Length; i++)
            {

                float rand = UnityEngine.Random.value;

                if (rand < 0.5f)
                {
                    newSetting[i] = Settings[i];
                }
                else
                {
                    newSetting[i] = otherSettings[i];
                }


            }

            return newSetting;
        }
    }




    internal static List<GenVel> NegativeSelection(List<GenVel> genVelLib)
    {
        genVelLib.Sort(SortByfitness);
        // remove the first 250
        genVelLib.RemoveRange(0, 250);

        return genVelLib;
    }

    private static int SortByfitness(GenVel x, GenVel y)
    {
        return x.fitness.CompareTo(y.fitness);
    }

    internal static List<GenKurmto> NegativeSelection(List<GenKurmto> genKurLib)
    {
        genKurLib.Sort(SortByfitness);
        // remove the first 250
        genKurLib.RemoveRange(0, 250);

        return genKurLib;
    }

    private static int SortByfitness(GenKurmto x, GenKurmto y)
    {
        return x.fitness.CompareTo(y.fitness);
    }
}
