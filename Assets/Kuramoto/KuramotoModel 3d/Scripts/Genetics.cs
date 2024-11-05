using UnityEngine;

public class Genetics
{
    public struct GenVel
    {
        public Vector3[] Vels;
        public float fitness;

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
                float rand = Random.value;

                if (rand < 0.33f)
                    newVels[i] = Vels[i];
                else if (rand < 0.66f)
                    newVels[i] = otherVels[i];
                else
                    newVels[i] = Random.insideUnitSphere;
            }

            return newVels;
        }
    }

    public struct GenKurmto
    {
        public float[] Settings;
        public float fitness;

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
                newSetting[i] = Random.value<0.5f?Settings[i]: otherSettings[i];

            return newSetting;
        }
    }

    public struct Antigen
    {
        public int[] Key;
        public float fitness;

        public Antigen(int[] SetKey = null, float fit = 0)
        {
            if (SetKey != null)
            {
                Key = SetKey;
            }
            else
            {
                Key = new int[5];
                for(int i=0; i<Key.Length; i++)
                    Key[i] = Random.Range(0, 10);
            }

            fitness = fit;
        }

        public Antigen(int keyLength)
        {
            Key = new int[keyLength];
            for (int i = 0; i < keyLength; i++)
                Key[i] = Random.Range(0, keyLength);
                        
            fitness = 0f;
        }
    }
}
