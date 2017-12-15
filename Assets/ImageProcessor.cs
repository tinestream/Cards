using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public interface FeaturesHolder {
	float [] Features { get; set; }
	float Ratio {get; set;}
	float Mean {get; set;}
	float Theta {get; set;}
	void copyFrom (FeaturesHolder holder);
}

[Serializable]
public class FeaturesClass : FeaturesHolder {
	public const int NFEAT = 3;
	protected float[] features;
	public float [] Features { 
		get { return features; } 
		set { 
			if (value == null || features.Length != value.Length)
				throw new Exception ("Length should be the same");
			for (int i = 0; i < features.Length; i++)
				features [i] = value [i];
		}}
	public float Ratio { get { return features [0]; } set { features [0] = value; } }
	public float Mean { get { return features [1]; } set { features [1] = value; } }
	public float Theta { get { return features [2]; } set { features [2] = value; } }
	public void copyFrom(FeaturesHolder holder) {
		for (int j = 0; j < NFEAT; j++) {
			features [j] = holder.Features[j];
		}
	}
	public FeaturesClass() {
		features = new float[NFEAT];
	}
}

[Serializable]
public class BoundingBox {
	private int [] bounds = new int[4];
	public int MinX { get { return bounds[0]; } set { bounds[0] = value; } }
	public int MinY { get { return bounds[1]; } set { bounds[1] = value; } }
	public int MaxX { get { return bounds[2]; } set { bounds[2] = value; } }
	public int MaxY { get { return bounds[3]; } set { bounds[3] = value; } }
	public int H { get { return bounds[3] - bounds[1]; } }
	public int W { get { return bounds[2] - bounds[0]; } }
	public BoundingBox(int minx_, int miny_, int maxx_, int maxy_) {
		bounds [0] = minx_;
		bounds [1] = miny_;
		bounds [2] = maxx_;
		bounds [3] = maxy_;
	}
	public void Union(BoundingBox box) {
		if (box.bounds [0] < bounds [0])
			bounds [0] = box.bounds [0];
		if (box.bounds [1] < bounds [1])
			bounds [1] = box.bounds [1];
		if (box.bounds [2] > bounds [2])
			bounds [2] = box.bounds [2];
		if (box.bounds [3] > bounds [3])
			bounds [3] = box.bounds [3];
	}
	public BoundingBox(BoundingBox box) : this(box.bounds) {
	}
	public BoundingBox(int [] box) {
		Bounds = box;
	}
	public void Shift(int x, int y) {
		bounds [0] += x;
		bounds [1] += y;
		bounds [2] += x;
		bounds [3] += y;
	}
	public void ScaleX(float scaleX) {
		bounds [0] = (int)(bounds [0] * scaleX);
		bounds [2] = (int)(bounds [2] * scaleX);
	}
	public void ScaleY(float scaleY) {
		bounds [1] = (int)(bounds [1] * scaleY);
		bounds [3] = (int)(bounds [3] * scaleY);
	}
	public int [] Bounds {
		get{ return bounds; }
		set{ 		
			for (int i = 0; i < 4; i++)
				bounds [i] = value [i];
		}
	}
}

public class Cluster : BoundingBox, FeaturesHolder {
	private FeaturesClass features = new FeaturesClass();
	public float [] Features { get { return features.Features; } set { features.Features = value;}}
	public float Ratio { get { return features.Ratio; } set { features.Ratio = value; } }
	public float Mean { get { return features.Mean; } set { features.Mean = value; } }
	public float Theta { get { return features.Theta; } set { features.Theta = value; } }
	public void copyFrom(FeaturesHolder holder) {
		features.copyFrom (holder);
	}
	private int size;
	private int id;
	public int Size { get { return size; } set { size = value; } }
	public int Id { get { return id; } }
	public Cluster(int id_, int size_, int minx_, int miny_, int maxx_, int maxy_) : base(minx_, miny_, maxx_, maxy_) {
		id = id_;
		size = size_;
	}
	/*private List<Pattern> recognized = new List<Pattern>(100);
	private List<float> scores = new List<float> (100);
	public void addRecognized(Pattern pat, float score) {
		recognized.Add (pat);
		scores.Add (score);
	}*/
}

[Serializable]
public abstract class Pattern {
	public const int BASE_ID = 1000000;
	private static int lastid = BASE_ID;
	public static int LastId { get {return lastid;}}
	public static int IncLastId() {
		lastid += BASE_ID;
		return lastid;
	}
	public static string GetName(int id) {
		id %= Pattern.BASE_ID;
		if (id < 256) {
			return new string ((char)id, 1);
		} else {
			id -= 2000;
			string res = "";
			while (id > 0) {
				char c = (char)(id % 256);
				id /= 256;
				res = new string (c, 1) + res;
			}
			if (res.Length > 0)
				return res;
			else
				return "#U";
		}	
	}
	protected int w;
	protected int h;
	public int W { get { return w; } }
	public int H { get { return h; } }
	public abstract float Tol { get; set; }
	protected int patternid;
	protected PatternBaseGroup parent = null;
	public int Id { get { return patternid; } }
	protected Pattern(int w_, int h_, int patternid_) {
		w = w_;
		h = h_;
		patternid = patternid_ + lastid;
	}
	public PatternBaseGroup Parent {get {return parent;} set {parent = value;}}
}


[Serializable]
public class PatternImage : Pattern, FeaturesHolder {
	private FeaturesClass features = new FeaturesClass();
	private float[] pattern;
	private float [] thrs = new float[2];
	public float [] Pattern { 
		get { return pattern; } 
		set { 
			if (value == null || pattern.Length != value.Length)
				throw new Exception ("Length should be the same");
			Array.Copy (value, pattern, pattern.Length);
		}}
	public override float Tol { get { return thrs[0]; } set { thrs[0] = value; } }
	public float Threshold { get { return thrs[1]; } set { thrs[1] = value; } }
	public float[] Thrs { get { return thrs; } }
	public float [] Features { get { return features.Features; } set { features.Features = value;}}
	public float Ratio { get { return features.Ratio; } set { features.Ratio = value; } }
	public float Mean { get { return features.Mean; } set { features.Mean = value; } }
	public float Theta { get { return features.Theta; } set { features.Theta = value; } }
	public void copyFrom(FeaturesHolder holder) {
		features.copyFrom (holder);
	}
	public PatternImage(int w_, int h_,  int patternid_) : base(w_, h_, patternid_) {
		pattern = new float[w_ * h_];
		thrs [0] = 0.9f;
		thrs [1] = 0.95f;
	}
	public PatternImage(int w_, int h_, float [] pattern_, int patternid_) : this(w_, h_, patternid_) {
		Array.Copy(pattern_, pattern, w_ * h_);
	}
}

[Serializable]
public abstract class PatternBaseGroup : Pattern {
	public const int MAX_GROUP = 1000;
	protected float tol;
	protected List<Pattern> patterns = new List<Pattern>(100);
	protected PatternBaseGroup(int w_, int h_, int patternid_) : base(w_, h_, patternid_) {
		tol = 0.9f;
	}
	protected PatternBaseGroup(int patternid_) : this(0,0,patternid_) {
	}
	public override float Tol { get { return tol; } set { tol = value; } }
	public virtual void add_pattern(int maxx, int maxy, Pattern pat) {
		if (maxx + 1 > w)
			w = maxx + 1;
		if (maxy + 1 > h)
			h = maxy + 1;
		patterns.Add(pat);
		pat.Parent = this;
	}
	public int Count {
		get{ return patterns.Count; }
	}
	public Pattern this[int key] {
		get{ return patterns [key];}
	}
	public IEnumerator<Pattern> GetEnumerator() {
		return patterns.GetEnumerator();
	}
	/*public bool IsSynchronized { get { return true; } }
	public void CopyTo(Array array, int ind) {
		patterns.CopyTo (array, ind);
	}
	public object SyncRoot { get { return null; } }*/
}

[Serializable]
public class PatternClass : PatternBaseGroup {
	public PatternClass(int patternid_) : base(patternid_) {
	}
	public void add_pattern(Pattern pat) {
		base.add_pattern(pat.W - 1, pat.H - 1, pat);
		//if (pat.W == 
	}
}

[Serializable]
public class PatternGroup : PatternBaseGroup {
	private List<BoundingBox> bounds = new List<BoundingBox>(MAX_GROUP);
	private float th = 0;
	public float Theta { get { return th; } set{ th = value; }}
	public PatternGroup(int patternid_) : base(patternid_) {
	}
	public void add_pattern(BoundingBox box, Pattern pat) {
		for (int i = 0; i < patterns.Count; i++) 
			if (patterns [i].Id == pat.Id 
				&& ((bounds[i].MinX - box.MinX) * (bounds[i].MaxX - box.MinX) < 0 || (bounds[i].MinX - box.MaxX) * (bounds[i].MaxX - box.MaxX) < 0 ||
					(bounds[i].MinX >= box.MinX && bounds[i].MaxX <= box.MaxX))
				&& ((bounds[i].MinY - box.MinY) * (bounds[i].MaxY - box.MinY) < 0 || (bounds[i].MinY - box.MaxY) * (bounds[i].MaxY - box.MaxY) < 0 ||
					(bounds[i].MinY >= box.MinY && bounds[i].MaxY <= box.MaxY))) {
				if (bounds[i].MinX > box.MinX)
					bounds[i].MinX = box.MinX;
				if (bounds[i].MinY > box.MinY)
					bounds[i].MinY = box.MinY;
				if (bounds[i].MaxX < box.MaxX)
					bounds[i].MaxX = box.MaxX;
				if (bounds[i].MaxY < box.MaxY)
					bounds[i].MaxY = box.MaxY;
				if (bounds[i].MaxX >= w)
					w = bounds[i].MaxX + 1;
				if (bounds[i].MaxY >= h)
					h = bounds[i].MaxY + 1;
				return;
			}

		bounds.Add (new BoundingBox (box));
		/*if (x < 0 || y < 0)
			throw new Exception ("Wrong group, x=" + x + ", y = " + y);*/
		base.add_pattern (box.MaxX, box.MaxY, pat);
	}
	public override void add_pattern(int maxx, int maxy, Pattern pat) {
		add_pattern (new BoundingBox (0, 0, maxx, maxy), pat);
	}
	public BoundingBox GetBox(int i) {
		return new BoundingBox(bounds[i]);
	}
}
	
public class RecognizedPattern : BoundingBox {
	private Pattern pattern;
	private float foundTheta;
	private float score;
	public Pattern Pattern { get { return pattern; } }
	public float FoundTheta { get { return foundTheta; } }
	public float Score { get { return score; } }
	public RecognizedPattern(Pattern pattern_, float foundTheta_, float score_, int minx_, int miny_, int maxx_, int maxy_) : base(minx_, miny_, maxx_, maxy_) {
		pattern = pattern_;
		foundTheta = foundTheta_;
		score = score_;
	}
	public virtual BoundingBox ClusterBox {
		get { return new BoundingBox(this); }
	}
}

public class RecognizedPatternImage : RecognizedPattern {
	private Cluster cluster;
	new public PatternImage Pattern { get { return (PatternImage) base.Pattern; } }
	public Cluster PCluster { get { return cluster; } }
	public RecognizedPatternImage(PatternImage pattern_, Cluster cluster_, float foundTheta_, float score_, int minx_, int miny_, int maxx_, int maxy_) : base(pattern_, foundTheta_, score_, minx_, miny_, maxx_, maxy_) {
		cluster = cluster_;
	}
	public override BoundingBox ClusterBox {
		get { return new BoundingBox(cluster); }
	}
}

public class RecognizedPatternGroup : RecognizedPattern {
	private RecognizedPattern [] recPattern;
	new public PatternGroup Pattern { get { return (PatternGroup) base.Pattern; } }
	//public RecognizedPattern [] RecPattern { get { return recPattern; } }
	public RecognizedPattern this[int key] {
		get{ return recPattern [key];}
		set{ recPattern [key] = value; }
	}
	public int Count {
		get { return recPattern.Length; }
	}
	public RecognizedPatternGroup(PatternGroup pattern_, float foundTheta_, float score_, int minx_, int miny_, int maxx_, int maxy_) : base(pattern_, foundTheta_, score_, minx_, miny_, maxx_, maxy_) {
		recPattern = new RecognizedPattern [pattern_.Count];
	}
	public override BoundingBox ClusterBox {
		get {
			BoundingBox res = new BoundingBox (recPattern [0].ClusterBox);
			for (int i = 1; i < recPattern.Length; i++)
				res.Union (recPattern [i].ClusterBox);
			return res;
		}
	}
}

public class ImageProcessor
{
	private WebCamTexture webCamTexture;
	private Texture2D texture;
	private int w;
	private int h;
	public const int UNKNOWN = 0;//'U';
	public ImageProcessor(int w_, int h_, WebCamTexture wct, Texture2D text) {
		w = w_;
		h = h_;
		webCamTexture = wct;
		texture = text;
		init_temp ();
	}
	private int GSIZE = 0;
	private int GDIV = 159;
	private int[,] GMAT = {{2, 4, 5, 4, 2}, {4, 9, 12, 9, 4}, {5, 12, 15, 12, 5}, {4, 9, 12, 9, 4}, {2, 4, 5, 4, 2}};

	private static float getGx(int ind, float [] cols, int w) {
		return (cols [ind - w + 1] - cols [ind - w - 1] +
			2 * (cols [ind + 1] - cols [ind - 1]) +
			cols [ind + w + 1] - cols [ind + w - 1]) / 8;
	}
	private static float getGx(int x, int y, float [] cols, int w) {
		return (cols [(y - 1) * w + x + 1] - cols [(y - 1) * w + x - 1] +
			2 * (cols [y * w + x + 1] - cols [y * w + x - 1]) +
			cols [(y + 1) * w + x + 1] - cols [(y + 1) * w + x - 1]) / 8;
	}

	private static float getGy(int ind, float [] cols, int w) {
		return (cols [ind + w - 1] - cols [ind - w - 1] +
			2 * (cols [ind + w] - cols [ind - w]) +
			cols [ind + w + 1] - cols [ind - w + 1]) / 8;
	}
	private static float getGy(int x, int y, float [] cols, int w) {
		return (cols [(y+1) * w + x - 1] - cols [(y - 1) * w + x - 1] +
			2 * (cols [(y+1) * w + x] - cols [(y-1) * w + x]) +
			cols [(y + 1) * w + x + 1] - cols [(y - 1) * w + x + 1]) / 8;
	}

	// Use this for initialization

	private void GaussFilter(float [] cls, float [] fxy) {
		if (GSIZE > 0) {
			for (int y = 0; y < h; y++)
				for (int x = 0; x < w; x++)
					for (int i = -GSIZE / 2; i <= GSIZE / 2; i++)
						if (y + i >= 0 && y + i < h)
							for (int j = -GSIZE / 2; j <= GSIZE / 2; j++)
								if (x + j >= 0 && x + j < w)
									fxy [y * w + x] += cls [(y + i) * w + x + j] * GMAT [i + GSIZE / 2, j + GSIZE / 2] / GDIV;
			Array.Copy (fxy, cls, cls.Length);

		}
	}

	private void LocalFilter(float sig, int iter, float [] fxy, float [] cls, float [] wt) {
		if (iter <= 0) 
			GaussFilter (cls, fxy);
		else
			for (int k = 0; k < iter; k++) {
				for (int x = 1; x < w - 1; x++)
					for (int y = 1; y < h - 1; y++) {
						float gx = getGx (x, y, cls, w);
						float gy = getGy (x, y, cls, w);
						float d = Mathf.Sqrt (gx * gx + gy * gy);
						wt [y * w + x] = Mathf.Exp (-Mathf.Sqrt (d) / sig / sig / 2); 
					}
				for (int x = 1; x < w - 1; x++)
					for (int y = 1; y < h - 1; y++) {
						float N = 0;
						fxy [y * w + x] = 0;
						for (int i = -1; i <= 1; i++)
							for (int j = -1; j <= 1; j++) {
								N += wt[(y + j) * w + x + i];
								fxy [y * w + x] += wt[(y + j) * w + x + i] * cls[(y + j) * w + x + i];
							}
						fxy [y * w + x] /= N;
					}
				for (int x = 1; x < w - 1; x++)
					for (int y = 1; y < h - 1; y++) {
						cls [y * w + x] = fxy [y * w + x];
					}
			}
	}

	private static void FindEdge(float [] fxy, float [] cls, float []theta, int x0, int y0, int x1, int y1, int w) {
		float maxg = 0;
		for (int y = y0; y <= y1; y++) 
			for (int x = x0; x <= x1; x++) {
				int ind = y * w + x;
				float gx = getGx (ind, cls, w);
				float gy = getGy (ind, cls, w);
				float fxyi = Mathf.Sqrt (gx * gx + gy * gy);
				fxy[ind] = fxyi;
				if (fxyi > maxg)
					maxg = fxyi;
				theta[ind] = Mathf.Atan2 (gy, gx);
			}
		//Debug.Log (maxg);
		for (int i = 0; i < cls.Length; i++)
			cls [i] = fxy [i] / maxg;
	}

	private static void SuppressNonmax(float [] cls, float [] fxy, float [] theta, int x0, int y0, int x1, int y1, int w) {
		Array.Copy (cls, fxy, cls.Length);
		for (int y = y0; y <= y1; y++)
			for (int x = x0; x <= x1; x++) {
				int ind = y * w + x;
				float th = theta [ind];
				float cl = cls [ind];
				float costh = Mathf.Cos (th);
				float sinth = Mathf.Sin (th);
				/*int dirx = (int)Mathf.Round (Mathf.Cos (th));
				int diry = ((int)Mathf.Round (Mathf.Sin (th))) * w;

				if (cl >= cls [ind + diry + dirx] && cl >= cls [ind - diry - dirx])
					fxy [ind] = cl;
				else
					fxy [ind] = 0;*/
				if (cl >= cls [Mathf.RoundToInt (y + sinth) * w + Mathf.RoundToInt (x + costh)] &&
					cl >= cls [Mathf.RoundToInt (y - sinth) * w + Mathf.RoundToInt (x - costh)]) 
					fxy [ind] = cl;
				else
					fxy [ind] = 0;
				//fxy [ind] = cl;
			}
	}

	private static void FindConnected(float thrmin, float thrmax, float [] fxy, int [] connect, List<Cluster> clusters, int x0, int y0, int xe, int ye, int w, int h) {
		int cllast = 1;
		clusters.Clear ();
		int[,] stack = new int[(xe - x0 + 1) * (ye - y0 + 1), 2];
		for (int y = y0; y <= ye; y++)
			for (int x = x0; x <= xe; x++) {
				int ind = y * w + x;
				if (fxy [ind] > thrmax && connect [ind] == 0) {
					int stcksz = 1;
					connect [ind] = cllast;
					Cluster cluster = new Cluster (clusters.Count, 1, x, y, x, y);
					clusters.Add (cluster);
					stack [stcksz - 1, 0] = y;
					stack [stcksz - 1, 1] = x;
					for (int ist = 0; ist < stcksz; ist++) {
						int y1 = stack [ist, 0];
						int x1 = stack [ist, 1];
						for (int i = -1; i <= 1; i++)
							if (i + y1 >= 0 && i + y1 < h - 1)
								for (int j = -1; j <= 1; j++)
									if (j + x1 >= 0 && j + x1 < w - 1 && connect [(y1 + i) * w + x1 + j] == 0 && fxy [(y1 + i) * w + x1 + j] > thrmin) {
										connect [(y1 + i) * w + x1 + j] = cllast;

										cluster.Size++;
										if (cluster.MinX > x1 + j)
											cluster.MinX = x1 + j;
										if (cluster.MinY > y1 + i) //can't happen?
											cluster.MinY = y1 + i;
										if (cluster.MaxX < x1 + j)
											cluster.MaxX = x1 + j;
										if (cluster.MaxY < y1 + i)
											cluster.MaxY = y1 + i;
										stack [stcksz, 0] = y1 + i;
										stack [stcksz, 1] = x1 + j;
										stcksz++;
									}
					}
					cllast++;
				}
			}
	}

	private int is_ccw(int p1, int p2, int p3, int[,] allpoints) {
		return (allpoints [p2, 0] - allpoints [p1, 0]) * (allpoints [p3, 1] - allpoints [p1, 1]) - (allpoints [p2, 1] - allpoints [p1, 1]) * (allpoints [p3, 0] - allpoints [p1, 0]);
	}
	private class AngleComparer : IComparer {
		int [,] points;
		int pointc;
		public AngleComparer(int [,] points_, int pointc){
			points = points_;
		}
		private int angle_cos(int x1, int y1, int x2, int y2) {
			return (x1 * x2 + y1 * y2);// sign is important / Mathf.Sqrt ((x1 * x1 + y1 * y1) * (x2 * x2 + y2 * y2));
		}
		public int Compare(object o1, object o2) {
			int i1 = (int)o1;
			int i2 = (int)o2;
			return angle_cos (points [i1, 0] - points [pointc, 0], points [i1 , 1] - points [pointc, 1], 
				points [i2, 0] - points [pointc, 0], points [i2, 1] - points [pointc, 1]);
			//return Mathf.Sign(angle);
		}
	}
	private int calcConvexHull(int i, List<Cluster> clusters, int [] connect, int [,] allpoints, int [] ind) { // Graham scan
		int len = 0;
		int minp = 1, miny = h, minx = w;
		Cluster cluster = clusters [i];
		for (int y = cluster.MinY; y <= cluster.MaxY; y++)
			for (int x = cluster.MinX; x <= cluster.MaxX; x++)
				if (connect [y * w + x] == i + 1) {
					len++;
					allpoints [len, 0] = x;
					allpoints [len, 1] = y;
					if (y < miny || (y == miny && x < minx)) {
						minp = len;
						minx = x;
						miny = y;
					}
				}
		if (minp != 1) {
			allpoints [minp, 0] = allpoints [1, 0];
			allpoints [minp, 1] = allpoints [1, 1];
			allpoints [1, 0] = minx;
			allpoints [1, 1] = miny;
		}
		for (int j = 0; j <= len; j++)
			ind [j] = j;
		Array.Sort (ind, 2, len - 1, new AngleComparer(allpoints, 1));
		allpoints [0, 0] = allpoints [len, 0];
		allpoints [0, 1] = allpoints [len, 1];
		for (int j = 0; j <= len; j++)
			if (ind [j] > len)
				Debug.Log ("Error! Ind[" + j + "]=" + ind [j] + ">" + len);

		int M = 1;
		for (int j = 2; j <= len; j++) {
			while (is_ccw (ind[M - 1], ind[M], ind[j], allpoints) <= 0) {
				if (M > 1) 
					M--;
				else if (j == len)
					break;
				else
					j++;
			}
			M++;
			int ind1 = ind [M];
			ind [M] = ind [j];
			ind [j] = ind1;
		}
		if (M > len)
			Debug.Log ("Error!! M = " + M + " > " +len);
		for (int j = 0; j <= len; j++)
			if (ind [j] > len)
				Debug.Log ("Error! Ind[" + j + "]=" + ind [j] + ">" + len);
		return M;
	}

	private void calc_new_box(float th, int width, int height, int [] newbounds) {
		float[,] edges = {{0, 0}, {width, 0}, {width, height}, {0, height}};
		//int[,] edgesto = new int[4, 2];
		newbounds[0] = (width + height)*2; newbounds[1] = newbounds[0]; newbounds[2] = -newbounds[0]; newbounds[3] = newbounds[2];
		for (int i = 0; i < 3; i++) {
			int edgestoi0 = (int)Math.Truncate((edges [i, 0] * Mathf.Cos (th) - edges [i, 1] * Mathf.Sin (th)));
			int edgestoi1 = (int)Math.Truncate((edges [i, 0] * Mathf.Sin (th) + edges [i, 1] * Mathf.Cos (th)));
			if (edgestoi0 < newbounds [0])
				newbounds [0] = edgestoi0;
			if (edgestoi1 < newbounds [1])
				newbounds [1] = edgestoi1;
			if (edgestoi0 + 1 > newbounds [2])
				newbounds [2] = edgestoi0 + 1;
			if (edgestoi1 + 1 > newbounds [3])
				newbounds [3] = edgestoi1 + 1;
		}
	}

	private void calc_new_box(float th, int minx, int miny, int maxx, int maxy, int [] groups, int [] connect, int [] newbounds) {
		newbounds [0] = (w + h) * 2; newbounds[1] = newbounds[0]; newbounds[2] = -newbounds[0]; newbounds[3] = newbounds[2];
		for (int y = 0; y <= maxy - miny; y++)
			for (int x = 0; x <= maxx - minx; x++)
				if (Array.BinarySearch(groups, 2, groups[1], connect [(y + miny) * w + x + minx]) >= 0) {
					int edgestoi0 = (int)Math.Truncate((x * Mathf.Cos (th) - y * Mathf.Sin (th)));
					int edgestoi1 = (int)Math.Truncate((x * Mathf.Sin (th) + y * Mathf.Cos (th)));
					if (edgestoi0 < newbounds [0])
						newbounds [0] = edgestoi0;
					if (edgestoi1 < newbounds [1])
						newbounds [1] = edgestoi1;
					if (edgestoi0 + 1 > newbounds [2])
						newbounds [2] = edgestoi0 + 1;
					if (edgestoi1 + 1 > newbounds [3])
						newbounds [3] = edgestoi1 + 1;
				}
	}

	private void calc_new_box(float th, int minx, int miny, int [] groups, int [] bl_st, int [,] blist, int [] newbounds) {
		newbounds [0] = (w + h) * 2; newbounds[1] = newbounds[0]; newbounds[2] = -newbounds[0]; newbounds[3] = newbounds[2];
		for (int i = 2; i < 2 + groups[1]; i++) {
			for (int j = bl_st [groups [i] - 1]; j < bl_st [groups [i]]; j++) {
				int edgestoi0 = (int)Math.Truncate(((blist[j, 0]-minx) * Mathf.Cos (th) - (blist[j, 1]-miny) * Mathf.Sin (th)));
				int edgestoi1 = (int)Math.Truncate(((blist[j, 0]-minx) * Mathf.Sin (th) + (blist[j, 1]-miny) * Mathf.Cos (th)));
				if (edgestoi0 < newbounds [0])
					newbounds [0] = edgestoi0;
				if (edgestoi1 < newbounds [1])
					newbounds [1] = edgestoi1;
				if (edgestoi0 + 1 > newbounds [2])
					newbounds [2] = edgestoi0 + 1;
				if (edgestoi1 + 1 > newbounds [3])
					newbounds [3] = edgestoi1 + 1;
			}
		}
	}

	private static float interpbilin (float x1, float y1, float [] cls, BoundingBox box) {
		x1 -= box.MinX;
		y1 -= box.MinY;
		int x1i = ((int) x1);
		int y1i = ((int) y1);
		int w = box.MaxX - box.MinX + 1;
		int h = box.MaxY - box.MinY + 1;
		/*if (w * h > cls.Length)
			throw new IndexOutOfRangeException ("Length must be: " + (w*h) + " and is: " + cls.Length);*/
		if (x1i >= 0 && x1i + 1 < w && y1i >= 0 && y1i + 1 < h) {
			return cls [y1i * w + x1i] + (cls [y1i * w + x1i + 1] - cls [y1i * w + x1i]) * (x1 - x1i) +
				(cls [(y1i + 1) * w + x1i] - cls [y1i * w + x1i]) * (y1 - y1i) +
				(x1 - x1i) * (y1 - y1i) * (cls [(y1i + 1) * w + x1i + 1] + cls [y1i * w + x1i] - cls [(y1i + 1) * w + x1i] - cls [y1i * w + x1i + 1]);

		} else
			return -1;
	}
		
	private static float rotateAndScale(float th, int wid, int hei, float [] cls, int minx, int miny, BoundingBox box, int [] newbounds, float [] tpl, int tplw, int tplh, int tplx, int tply, float [] pat, float minMean, float thres) {
		float dist = 0;
		float norm = 0, norm1 = 0;
		for (int y = 0; y < hei; y++)
			for (int x = 0; x < wid; x++) {
				float xr = 1.0f * x * (newbounds [2] - newbounds [0]) / wid + newbounds[0];
				float yr = 1.0f * y * (newbounds [3] - newbounds [1]) / hei + newbounds[1];
				float x1 = ((xr * Mathf.Cos (-th) - yr * Mathf.Sin (-th)));
				float y1 = ((xr * Mathf.Sin (-th) + yr * Mathf.Cos (-th)));
				float tpln = interpbilin (x1 + minx, y1 + miny, cls, box);
				if (tpln >= 0 && tpl != null && y + tply >= 0 && y + tply < tplh && x + tplx >= 0 && x + tplx < tplw) {
					tpl [(y + tply) * tplw + (x + tplx)] = tpln;
				}
				if (minMean < 0.5 && tpln >= 0)
					tpln = 1 - tpln;
				if (pat != null) {
					float tpat = pat [y * wid + x];
					if (minMean < 0.5)
						tpat = 1 - tpat;
					/*if (tpat < 0.2)
						tpat = 0;
					else
						tpat = 1.0f;
					if (tpln < 0.2)
						tpln = 0;
					else
						tpln = 1.0f;*/
					if (tpln < 0)
						dist += Mathf.Max ((1.0f - tpat) * (1.0f - tpat), tpat * tpat);
					else
						dist += (tpln - tpat) * (tpln - tpat);
					norm1 += tpat * tpat; 
					/*if (thres > 0 && Mathf.Sqrt(dist) / norm > thres) {
						return dist;
					}*/
				} 
				if (tpln >= 0) {
					norm += tpln * tpln;
				} else
					norm++;
			}
		if (pat == null)
			return norm;
		else			
			return dist / Mathf.Min(norm, norm1);
	}
	private float rotateAndScale(float th, int wid, int hei, float [] cls, int minx, int miny, int [] newbounds, float [] tpl, float [] pat, float minMean, float thres) {
		return rotateAndScale (th, wid, hei, cls, minx, miny, new BoundingBox(0, 0, w - 1, h - 1), newbounds, tpl, wid, hei, 0, 0, pat, minMean, thres);
	}

	public static void RotateAndScale(float th, BoundingBox box, int origX, int origY, int groupW, int groupH, float [] to, PatternImage pat) {
		int w = (box.MaxX - box.MinX + 1), h = (box.MaxY - box.MinY + 1), w2 = w / 2, h2 = h / 2;
		rotateAndScale (th, w, h, pat.Pattern, origX, origY, new BoundingBox (origX-w2, origY-h2, origX-w2 + pat.W - 1, origY-h2 + pat.H - 1), new int[]{-w2, -h2, -w2+w - 1, -h2+h - 1}, to, groupW, groupH, box.MinX, box.MinY, null, 1f, 0);
	}


	private static int dist(int x1, int y1, int x2, int y2) {
		return (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
	}

	private float findTheta(float [] theta, int groupa, List<Cluster> clusters, int thsize) {
		int[] thetas = new int[thsize];
		int maxthetac = 0, maxth = 0;
		Cluster cluster = clusters[groupa - 1];
		for (int y = cluster.MinY; y <= cluster.MaxY; y++)
			for (int x = cluster.MinX; x <= cluster.MaxX; x++) {
				float th = theta [y * w + x]; //rotations of top left corner 
				while (th < 0 && Mathf.Abs (th) > Mathf.PI / 4)
					th += Mathf.PI / 2;
				while (th > 0 && Mathf.Abs (th) > Mathf.PI / 4)
					th -= Mathf.PI / 2;
				int thi = (int)Math.Truncate (thsize * (th + Mathf.PI / 2) / Mathf.PI);
				thetas [thi]++;
				if (thetas [thi] > maxthetac) {
					maxthetac = thetas [thi];
					maxth = thi;
				}
			}
		return (maxth * Mathf.PI / thsize - Mathf.PI / 2);			
	}
	private float findTheta(int [] groups, int minx, int miny, int [] bl_st, int [,] blist) {
		int minarea = w * h;
		float resth = 0;
		int[] newbox = new int[4];
		for (int i = 0; i < groups [1]; i++) {
			for (int j = bl_st [groups[i + 2] - 1]; j < bl_st [groups[i + 2]]; j++) {
				float th = 0;
				if (j > bl_st [groups [i + 2] - 1])
					th = Mathf.Atan2 (blist [j, 1] - blist [j - 1, 1], blist [j, 0] - blist [j - 1, 0]);
				else
					th = Mathf.Atan2 (blist [bl_st[groups[i + 2]]-1, 1] - blist [j, 1], blist [bl_st[groups[i + 2]]-1, 0] - blist [j, 0]);
				calc_new_box (-th, minx, miny, groups, bl_st, blist, newbox);
				int area = (newbox [2] - newbox [0]) * (newbox [3] - newbox [1]);
				if (area < minarea) {
					minarea = area;
					resth = th;
				}
			}
		}
		return resth;
	}
	private void calcFeatures(List<Cluster> clusters, int [] bl_st, int [,] blist, float [] filtered) {
		int[] newbox = new int[4];
		int[] gr = new int[3];
		gr [1] = 1;
		for (int i = 0; i < clusters.Count; i++)
			if (CheckCluster (i)) {
				gr [2] = i + 1;
				Cluster cluster = clusters [i];
				float th = findTheta (gr, cluster.MinX, cluster.MinY, bl_st, blist);
				calc_new_box (-th, cluster.MinX, cluster.MinY, gr, bl_st, blist, newbox);
				cluster.Ratio = 1f * (newbox [3] - newbox [1]) / (newbox [2] - newbox [0]);
				if (cluster.Ratio < 1)
					cluster.Ratio = 1 / cluster.Ratio;
				cluster.Mean = (Mathf.Sqrt (rotateAndScale (-th, (newbox [2] - newbox [0]), (newbox [3] - newbox [1]), filtered, cluster.MinX, cluster.MinY, newbox, null, null, 1f, 0)) / (newbox [2] - newbox [0]) / (newbox [3] - newbox [1])); 
				cluster.Theta = th;
			}
	}

	private float fixTheta(float thetaturn, float sector) {
		while (thetaturn < 0 && Mathf.Abs (thetaturn) > sector)
			thetaturn += Mathf.PI / 2;
		while (thetaturn > 0 && Mathf.Abs (thetaturn) > sector)
			thetaturn -= Mathf.PI / 2;
		return thetaturn;
	}

	private static int dist(BoundingBox cluster, int cenx, int ceny) {
		int minD = 4 * (cenx * cenx + ceny * ceny);
		if (cenx >= cluster.MinX && cenx <= cluster.MaxX && ceny >= cluster.MinY && ceny <= cluster.MaxY) {
			minD = 0;
			return minD;
		} 

		int d = dist (cluster.MinX, cluster.MinY, cenx, ceny);
		if (d < minD) {
			minD = d;
		}
		d = dist (cluster.MaxX, cluster.MinY, cenx, ceny);
		if (d < minD) {
			minD = d;
		}
		d = dist (cluster.MaxX, cluster.MaxY, cenx, ceny);
		if (d < minD) {
			minD = d;
		}
		d = dist (cluster.MinX, cluster.MaxY, cenx, ceny);
		if (d < minD) {
			minD = d;
		}	
		return minD;
	}

	private int dist(BoundingBox cluster1, BoundingBox cluster2) {
		int minD = dist (cluster1, cluster2.MinX, cluster2.MinY);
		int d = dist (cluster1, cluster2.MaxX, cluster2.MinY);
		if (d < minD) {
			minD = d;
		}
		d = dist (cluster1, cluster2.MaxX, cluster2.MaxY);
		if (d < minD) {
			minD = d;
		}
		d = dist (cluster1, cluster2.MinX, cluster2.MaxY);
		if (d < minD) {
			minD = d;
		}
		return minD;
	}

	private Pattern findPattern(int boxsize, int minsize, int cenx, int ceny, int [] connect, float [] filtered, List<Cluster> clusters, int [] bl_st, int [,] blist, int [] groups, int [] ids) {
		float thetaturn = 0;
		PatternGroup group = null;
		PatternImage pattern = null;
		int minD = (w+h)*(w+h);
		int mini = -1;
		for (int i = 0; i < clusters.Count; i++)
			if (CheckCluster (i)) {
				Cluster cluster = clusters [i];
				int clusterW = cluster.MaxX - cluster.MinX, clusterH = cluster.MaxY - cluster.MinY;
				if (clusterW < boxsize && clusterH < boxsize &&
				   (clusterW >= minsize || clusterH >= minsize) && (clusterW > 3 && clusterH > 3)) {
					int d = dist (cluster, cenx, ceny);
					if (d < minD) {
						mini = i;
						minD = d;
						if (d == 0)
							break;
					}
				}
			}
		groups [0] = 0;
		groups [1] = 0;
		if (mini >= 0) {
			Cluster cluster = clusters [mini];
			int minx = cluster.MinX, miny = cluster.MinY, maxx = cluster.MaxX, maxy = cluster.MaxY;
			int glen = 2;
			for (int i = 0; i < clusters.Count; i++)
				if (CheckCluster (i)) {
					if (mini != i) {
						cluster = clusters [i];
						int clusterW = cluster.MaxX - cluster.MinX, clusterH = cluster.MaxY - cluster.MinY;
						if ((clusterW >= minsize || clusterH >= minsize) && (clusterW > 3 && clusterH > 3)) {
							int minx1 = Mathf.Min (minx, cluster.MinX), miny1 = Mathf.Min (miny, cluster.MinY), 
							maxx1 = Mathf.Max (maxx, cluster.MaxX), maxy1 = Mathf.Max (maxy, cluster.MaxY);
							if (maxx1 - minx1 < boxsize && maxy1 - miny1 < boxsize) {
								minx = minx1;
								miny = miny1;
								maxx = maxx1;
								maxy = maxy1;
								groups [glen] = i + 1;
								glen++;
							} 
						}
					} else {
						groups [glen] = i + 1;
						glen++;
					}
				}
			groups [1] = glen - 2;
			int groupa = 0;
			for (int x = minx; x > 0 && x >= minx - (maxx - minx) * 2 && groupa == 0; x--)
				groupa = connect [miny * w + x];
			groups [0] = groupa;
			/*if (groupa > 0) {
				thetaturn = findTheta (theta, groupa, colorinf, thsize);
			}*/


			int[] newbox = new int[4]; 
			int[] newgroupbox = new int[4];
			int totgroups = groups [1];
			if (totgroups > 1) {
				thetaturn = fixTheta (findTheta (groups, minx, miny, bl_st, blist), Mathf.PI / 4);

				calc_new_box (-thetaturn, minx, miny, groups, bl_st, blist, newgroupbox);

				for (int i = 0; i < groups [1]; i++) 
					if (groups[2 + i] > 0) {
						Cluster clusteria = clusters [groups [2 + i] - 1];
						for (int j = i + 1; j < groups [1]; j++) 
							if (groups[2 + j] > 0) {
								Cluster clusteri = clusteria;
								Cluster clusterj = clusters [groups [2 + j] - 1];
								if (clusteri.MinY > clusterj.MinY) {
									clusteri = clusterj;
									clusterj = clusteria;
								}
								if (clusteri.MaxY >= clusterj.MaxY && clusteri.MinX <= clusterj.MinX &&
								    clusteri.MaxX >= clusterj.MaxX) {
									totgroups--;
									if (clusteri == clusteria) {
										groups [2 + j] = 0;
									} else {
										groups [2 + i] = 0;
										break;
									}
								}
							}
					}
				if (totgroups > 1) {
					int grid = 0;
					for (int i = 0; i < ids.Length; i++)
						grid = grid * 256 + ids [i];
					if (grid == 0)
						grid = UNKNOWN;
					grid += 2000;
					group = new PatternGroup (grid);
					group.Theta = thetaturn;
				}
				/*int mingroupi = 0;
				Cluster mincluster = clusters [groups [2] - 1];
				for (int i = 1; i < groups [1]; i++) {
					Cluster clusteri = clusters [groups [2 + i] - 1];
					if (clusteri.MinX < mincluster.MinX)
						;
				}*/
			}

			int[] gr = new int[3];
			gr [1] = 1;
			int i1 = 0;
			for (int i = 0; i < groups [1]; i++) 
				if (groups[2 + i] > 0) {
					int id = UNKNOWN;
					if (i1 < ids.Length)
						id = ids [i1];
					gr [2] = groups [2 + i];
					cluster = clusters [groups [2 + i] - 1];
					float thetaturn1 = fixTheta (cluster.Theta, Mathf.PI / 4);
					calc_new_box (-thetaturn1, cluster.MinX, cluster.MinY, gr, bl_st, blist, newbox); 
					pattern = new PatternImage(newbox[2]-newbox[0], newbox[3]-newbox[1], id);
					rotateAndScale (-thetaturn1, pattern.W, pattern.H, filtered, cluster.MinX, cluster.MinY, newbox, pattern.Pattern, null, 1f, 0);
					pattern.copyFrom (cluster);
					pattern.Theta = thetaturn1;
					//Debug.Log ("Pattern: " + (cluster.MaxX - cluster.MinX) + "," + (cluster.MaxY - cluster.MinY) + ";" + (newbox[2]-newbox[0]) + "," + (newbox[3]-newbox[1]));
					if (group != null) {
						calc_new_box (-thetaturn, minx, miny, gr, bl_st, blist, newbox); 
						group.add_pattern (new BoundingBox(newbox[0] - newgroupbox[0], newbox[1] - newgroupbox[1], newbox[2] - newgroupbox[0],
							newbox[3] - newgroupbox[1]), pattern);
					} 
					i1++;
				}
			if (i1 != totgroups)
				throw new Exception ("Expected " + totgroups + " groups, found " + i1);

			//calc_new_box (-thetaturn, minx, miny, maxx, maxy, groups, connect, newbox);
			//Debug.Log ("Width: " + (newbox[2] - newbox[0]) + ", Height: " + (newbox[3] - newbox[1]));
			//Debug.Log ("Features(" + groups[1] + "): " + features [0, 0] + ", " + features [0, 1]);
		}
		if (group == null)
			return pattern;
		else
			return group;
	}


	private int recognizePattern(PatternImage pat, float [] thrs, int minsize, int maxsize, int [] connect, float [] filtered, List<Cluster> clusters, int[] bl_st, int[,] blist, int [] patterns, float [] thetas, float [] scores, int [,] bounds) {
		int[] newbox = new int[4]; 
		int[] groups = new int[clusters.Count + 2];
		float[] pat1 = new float[pat.W * pat.H];
		groups [1] = 1;
		float mindi = 10000000;
		int resid = 0, ncomp = 0;
		float nerr = thrs[0], thr = 1 - thrs[1];
		int len = 0;
		//Debug.Log ("a: " + minsize + " " + maxsize);
		for (int i = 0; i < clusters.Count; i++)
			if (CheckCluster (i)) {
				Cluster cluster = clusters [i];
				if (cluster.W <= maxsize && cluster.H <= maxsize &&
				   (cluster.H >= minsize || cluster.W >= minsize) && (cluster.H > 3 && cluster.W > 3)//) {
				   && (cluster.Ratio / pat.Ratio < 1 / nerr && cluster.Ratio / pat.Ratio > nerr)) {
					//&& (all_features[i, 1]/features[0, 1] < 1/ nerr && all_features[i, 1]/features[0, 1] > nerr)) {
					ncomp++;
					groups [2] = i + 1;
					for (int thi = 0; thi < 4; thi++) {
						//float di = 0;
						float th = -cluster.Theta + (Mathf.PI / 2 * thi);
						//calc_new_box (th, colorinf[i, 1], colorinf[i, 2], colorinf[i, 3], colorinf[i, 4], groups, connect, newbox);
						calc_new_box (th, cluster.MinX, cluster.MinY, groups, bl_st, blist, newbox);
						float scale = (pat.W * 1.0f / pat.H) / ((newbox [2] - newbox [0]) * 1f / (newbox [3] - newbox [1]));
						if (scale < 1)
							scale = 1 / scale;
						if (scale > 1 / nerr)
							continue;
						//Debug.Log ("Width: " + (newbox[2] - newbox[0]) + ", Height: " + (newbox[3] - newbox[1]));
						float minMean = Mathf.Min (pat.Mean, cluster.Mean);
						float di = rotateAndScale (th, pat.W, pat.H, filtered, cluster.MinX, cluster.MinY, newbox, pat1, pat.Pattern, minMean, 1.5f * thr);
						di = Mathf.Sqrt (di) * scale;
						/*for (int y = 0; y < patternh; y++)
						for (int x = 0; x < patternw; x++) {
							di += (pat1 [y * patternw + x] - pattern [y * patternw + x]) * (pat1 [y * patternw + x] - pattern [y * patternw + x]);
						}*/

						if (di < mindi) {
							mindi = di;
							resid = i + 1;
						}
						if (di < thr) {
							patterns [len] = i + 1;
							scores [len] = di;
							thetas [len] = th;
							for (int j = 0; j < 4; j++) {
								bounds [len, j] = newbox [j];
							}
							len++;
							break;
						}
					}
				}
			}
		if (false)//resid > 0)
			Debug.Log ("Comp number: " + ncomp + ". Min score: " + mindi + ". Features: " + pat.Ratio + "," + pat.Mean +"; " + 
				clusters[resid - 1].Ratio + "," + clusters[resid - 1].Mean);
		if (mindi > thr)
			resid = 0;
		return len;
	}

	private float[] wt;
	private float[] cls;
	private float[] filtered;
	private float[] fxy;
	private float[] theta;
	private int[] connect;
	private List<Cluster> clusters;//sorted by y asc
	private int[] boundpoints_st;
	private int[, ] boundpoints_list;
	private int[,] templist;
	private int[] tempid;
	private void init_temp() {
		wt = new float[w * h];
		cls = new float[w * h];
		filtered = new float[w * h];
		fxy = new float[w * h];
		theta = new float[w * h];
		connect = new int [w * h];
		clusters = new List<Cluster> (w * h);
		boundpoints_st = new int [w * h]; //clusters.Count + 1
		boundpoints_list = new int[w * h, 2];
		templist = new int[w * h, 2];
		tempid = new int[w * h];
	}

	private bool CheckCluster(int i) {
		return clusters [i].Size > 20;
	}

	private void process_image(float minthr, float maxthr, float filter, int drawminsize) {

		Color[] color = webCamTexture.GetPixels ();

		//Debug.Log (w + " " + h);
		for (int i = 0; i < color.Length; i++) {
			cls [i] = color [i].grayscale;
			wt [i] = 0f;
			fxy [i] = 0f;
		}
		if (filter == 0) {
			GSIZE = 5;
			filter = 10;
		}
		LocalFilter (filter, filter == 10 ? 0 : 2, fxy, cls, wt);
		for (int i = 0; i < cls.Length; i++) {
			filtered [i] = cls [i];
			fxy [i] = cls [i];
		}
		FindEdge (fxy, cls, theta, 1, 1, w - 2, h - 2, w);
		SuppressNonmax (cls, fxy, theta, 1, 1, w - 2, h - 2, w);

		FindConnected (minthr, maxthr, fxy, connect, clusters, 1, 1, w - 2, h - 2, w, h);

		int len_bp = 0;

		int numcl = 0;
		for (int i = 0; i < clusters.Count; i++) {
			boundpoints_st [i] = len_bp; 
			if (CheckCluster(i))
				numcl++;
			int leni = calcConvexHull (i, clusters, connect, templist, tempid);
			for (int j = 0; j < leni; j++) {
				boundpoints_list [len_bp + j, 0] = templist [tempid [j + 1], 0];
				boundpoints_list [len_bp + j, 1] = templist [tempid [j + 1], 1];
			}
			len_bp += leni;
		}
		boundpoints_st [clusters.Count] = len_bp;
		if (false)
			Debug.Log ("Number of colors: " + clusters.Count + "," + numcl);
		calcFeatures (clusters, boundpoints_st, boundpoints_list, filtered);

		for (int y = 0, i = 0; y < h; y++)
			for (int x = 0; x < w; x++, i++) {
				if (fxy [i] > minthr && (drawminsize == 0 || (connect [y * w + h] > 0 &&
				    (clusters [connect [y * w + h] - 1].W >= drawminsize || clusters [connect [y * w + h] - 1].H >= drawminsize))))// * maxg)
					color [i] = new Color (fxy [i], fxy [i], fxy [i]);
				else
					color [i] = new Color (0, 0, 0);
			}
		texture.SetPixels (color);

		/*for (int i = 0; i < cornco; i++) {
					double th = corners [i, 0] * 2 * Mathf.PI / thsize - Mathf.PI;
					for (int j = 0; j < 200; j++) {
						int x = (int)Math.Truncate (corners [i, 1] * xmesh + j * Mathf.Cos (th)), 
						y = (int)Math.Truncate (corners [i, 2] * ymesh + j * Mathf.Sin (th));
						if (x >= 0 && x < w && y >= 0 && y < h)
							texture.SetPixel (x, y, new Color (corners [i, 3] / 100.0f, 0, 0));
						x = (int)Math.Truncate (corners [i, 1] * xmesh + j * Mathf.Cos (th - Mathf.PI / 2));
						y = (int)Math.Truncate (corners [i, 2] * ymesh + j * Mathf.Sin (th - Mathf.PI / 2));
						//if (x >= 0 && x < w && y >= 0 && y < h)
						//	texture.SetPixel (x, y, new Color(1, 0, 0));
					}
				}*/
		for (int i = 0; i < clusters.Count; i++)
			if (CheckCluster(i) && (drawminsize == 0 || clusters[i].H >= drawminsize || clusters[i].W >= drawminsize)) {
				Cluster cluster = clusters [i];
				for (int x = cluster.MinX; x <= cluster.MaxX; x++) {
					texture.SetPixel (x, cluster.MinY, new Color (1.0f, 0, 0));
					texture.SetPixel (x, cluster.MaxY, new Color (1.0f, 0, 0));
				}
				for (int y = cluster.MinY; y <= cluster.MaxY; y++) {
					texture.SetPixel (cluster.MinX, y, new Color (1.0f, 0, 0));
					texture.SetPixel (cluster.MaxX, y, new Color (1.0f, 0, 0));
				}
			}
	}

	public void DrawCaptureBox(int captSize) {
		for (int i = w / 2 - captSize / 2; i < w / 2 + captSize / 2; i++)
			for (int j = h / 2 - captSize / 2; j < h / 2 + captSize / 2; j++)
				if ((j - h / 2) * (j - h / 2) > captSize / 2 * captSize / 2 - 100 || (i - w / 2) * (i - w / 2) > captSize / 2 * captSize / 2 - 100)
					texture.SetPixel (i, j, new Color (0.5f, 1.0f, 0.5f));
	}

	public void ProcessImage(int captSize, float minthr, float maxthr, float filter, int minDrawSize = 0) {
		process_image (minthr, maxthr, filter, minDrawSize);
		DrawCaptureBox (captSize);
	}

	public Pattern FindLowLevelPattern(int captSize, int minSize, string text) {
		int[] groups = new int [clusters.Count];
		int[] ids = new int[text.Length];
		for (int i = 0; i < ids.Length; i++)
			ids [i] = text [i];
		Pattern pat = findPattern (captSize, minSize, w / 2, h / 2, connect, filtered, clusters, boundpoints_st, boundpoints_list, groups, ids);
		if (groups [1] > 0) {
			for (int i = 0; i < groups [1]; i++) 
				if (groups[i + 2] > 0) {
					int groupb = groups [i + 2];
					Cluster cluster = clusters [groupb - 1];
					for (int x = cluster.MinX; x <= cluster.MaxX; x++) {
						texture.SetPixel (x, cluster.MinY, new Color (0.0f, 1f, 1f));
						texture.SetPixel (x, cluster.MaxY, new Color (0.0f, 1f, 1f));
					}
					for (int y = cluster.MinY; y <= cluster.MaxY; y++) {
						texture.SetPixel (cluster.MinX, y, new Color (0.0f, 1f, 1f));
						texture.SetPixel (cluster.MaxX, y, new Color (0.0f, 1f, 1f));
					}
				}
		}
		int groupa = groups [0];
		if (groupa > 0) {
			Cluster cluster = clusters [groupa - 1];
			for (int x = cluster.MinX; x <= cluster.MaxX; x++) {
				texture.SetPixel (x, cluster.MinY, new Color (0.0f, 0f, 1f));
				texture.SetPixel (x, cluster.MaxY, new Color (0.0f, 0f, 1f));
			}
			for (int y = cluster.MinY; y <= cluster.MaxY; y++) {
				texture.SetPixel (cluster.MinX, y, new Color (0.0f, 0f, 1f));
				texture.SetPixel (cluster.MaxX, y, new Color (0.0f, 0f, 1f));
			}
			float thetaturn = 0;
			if (pat is PatternImage)
				thetaturn = ((PatternImage)pat).Theta;
			else if (pat is PatternGroup)
				thetaturn = ((PatternGroup)pat).Theta;
			for (int i = 0; i < 50; i++) {
				texture.SetPixel ((int)Math.Truncate (cluster.MinX + i * Mathf.Cos (thetaturn)), (int)Math.Truncate (cluster.MinY + i * Mathf.Sin (thetaturn)), new Color (1.0f, 0f, 1f));
			}
		}
				
		return pat;
	}
	private static void drawRecognizedCluster(Texture2D texture, BoundingBox cluster, float score, float minscore) {
		for (int x = cluster.MinX; x <= cluster.MaxX; x++) {
			texture.SetPixel (x, cluster.MinY, new Color (0.0f, (1f - score) / (1f - minscore), 0f));
			texture.SetPixel (x, cluster.MaxY, new Color (0.0f, (1f - score)/ (1f - minscore), 0f));
		}
		for (int y = cluster.MinY; y <= cluster.MaxY; y++) {
			texture.SetPixel (cluster.MinX, y, new Color (0.0f, (1f - score) / (1f - minscore), 0f));
			texture.SetPixel (cluster.MaxX, y, new Color (0.0f, (1f - score) / (1f - minscore), 0f));
		}
	}
	public void RecognizePatterns(int minSize, int maxSize, List<PatternImage> allPatterns, List<int> indst, List<RecognizedPatternImage> all, PatternImage drawPattern) {
		int[] listids = new int[clusters.Count];
		int[] patterns = new int[clusters.Count];
		float[] scores = new float[clusters.Count];
		float[] thetas = new float[clusters.Count];
		int[,] bounds = new int [clusters.Count, 4];
		indst.Clear ();
		all.Clear ();
		for (int j = 0; j < allPatterns.Count; j++) {
			int lenpat = recognizePattern (allPatterns[j], allPatterns[j].Thrs, minSize, maxSize, connect, filtered, clusters, boundpoints_st, boundpoints_list, patterns, thetas, scores, bounds);
			for (int i = 0; i < lenpat; i++)
				listids [i] = i;
			Array.Sort<float, int> (scores, listids, 0, lenpat);
			indst.Add(all.Count);
			//Debug.Log ("Pattern " + j + ": " + allPatterns [j].Thrs [0] + "," + allPatterns[j].Thrs [1] + ". Count: " + lenpat);
			for (int i = 0; i < lenpat; i++) {
				all.Add (new RecognizedPatternImage (allPatterns [j], clusters [patterns [listids[i]] - 1], thetas [listids[i]], scores [i], 
					bounds[listids[i], 0], bounds[listids[i], 1], bounds[listids[i], 2], bounds[listids[i], 3]));
			}
			if (drawPattern != null && drawPattern.Id == allPatterns [j].Id) {
				for (int i = 0; i < lenpat; i++) { 
					int squareind = patterns [listids[i]];
					drawRecognizedCluster (texture, clusters [squareind - 1], scores [i], scores [0]);
				}
			}
		}
		indst.Add(all.Count);
	}

	private class RPIDistanceComparer : IComparer {
		List<int> indst;
		List<RecognizedPatternImage> recognized;
		int cenx;
		int ceny;
		public RPIDistanceComparer(int cenx_, int ceny_, List<int> indst_, List<RecognizedPatternImage> recognized_){
			cenx = cenx_;
			ceny = ceny_;
			indst = indst_;
			recognized = recognized_;
		}
		public int Compare(object o1, object o2) {
			int i1 = (int)o1;
			int i2 = (int)o2;
			return 0;
			//return Mathf.Sign(angle);
		}
	}


	private float checkGroup(PatternGroup group, int [] grind, int [] groups, BoundingBox box, int [] bl_st, int [,] blist, int [] anewgroupbox) {
		float thetaturn = fixTheta (findTheta (groups, box.MinX, box.MinY, bl_st, blist), Mathf.PI / 4);

		int []  agroupbox = new int[4];
		int[] gr = new int[groups.Length];
		gr [1] = 1;
		float checkTheta = 1001f;
		for (int k = 0; k < 4 && checkTheta > 1000; k++, thetaturn += (Mathf.PI / 2)) {
			calc_new_box (-thetaturn, box.MinX, box.MinY, groups, bl_st, blist, anewgroupbox);
			BoundingBox newgroupbox = new BoundingBox(anewgroupbox);
			float gratio = 1f * group.W / group.H, ratio = 1f * newgroupbox.W / newgroupbox.H;
			float cratio = ratio / gratio;
			if (cratio > 1)
				cratio = 1 / cratio;
			if (cratio < group.Tol) {
				//if (grind != null)
				//Debug.Log ("Ratio: " + cratio + "," + group.Tol);
				continue;
			}
			float scale = 1f * group.W / newgroupbox.W;
			checkTheta = thetaturn;

			for (int i = 0; i < group.Count; i++) {
				if (grind != null) {
					gr [1] = grind [i + 1] - grind [i];
					for (int j = grind [i]; j < grind [i + 1]; j++)
						gr [2 + j - grind [i]] = groups [2 + j];
				} else
					gr [2] = groups [i + 2];
				calc_new_box (-thetaturn, box.MinX, box.MinY, gr, bl_st, blist, agroupbox);
				BoundingBox groupbox = new BoundingBox (agroupbox);
				BoundingBox gbox = group.GetBox (i);
				float tol = group.Tol;// [i].Tol;
				gratio = 1f * gbox.W / gbox.H;
				ratio = 1f * groupbox.W / groupbox.H;
				float cratio1 = ratio / gratio;
				if (cratio1 > 1)
					cratio1 = 1 / cratio1;
				if (cratio1 < tol) {
					checkTheta = 1001f;
					//if (grind != null)
					//Debug.Log ("Ratio " + i + ": " + cratio1 + "," + tol);
					break;	
				}
				float tolx = group.W * (1 - tol), toly = group.H * (1 - tol);
				float minx = (groupbox.MinX - newgroupbox.MinX) * scale, miny = (groupbox.MinY - newgroupbox.MinY) * scale, 
				maxx = (groupbox.MaxX - newgroupbox.MinX) * scale, maxy = (groupbox.MaxY - newgroupbox.MinY) * scale;
				if (Mathf.Abs (minx - gbox.MinX) > tolx || Mathf.Abs (miny - gbox.MinY) > toly ||
				    Mathf.Abs (maxx - gbox.MaxX) > tolx || Mathf.Abs (maxy - gbox.MaxY) > toly) {
					checkTheta = 1001f;
					/*if (grind != null)
					Debug.Log ("Ratio " + i + ": " + cratio1 + "," + tol + ". Positions: " + Mathf.Abs (minx - gbox.MinX) + "," + Mathf.Abs (miny - gbox.MinY) + "," +
						Mathf.Abs (maxx - gbox.MaxX) + "," +  Mathf.Abs (maxy - gbox.MaxY) + "; " + tolx + "," + toly);*/
					break;
				}
			}
		}
		return checkTheta;
	}

	private static int maxsize(BoundingBox box1, BoundingBox box2) {
		BoundingBox box = new BoundingBox (box1);
		box.Union (box2);
		return Mathf.Max (box.W, box.H);
	}

	private void eraseGroup(List<RecognizedPatternGroup> rpglist, int [] found, RecognizedPatternImage rpi) {
		if (found [rpi.PCluster.Id] >= 0) {
			RecognizedPatternGroup rpg = rpglist [found [rpi.PCluster.Id]];
			rpglist [found [rpi.PCluster.Id]] = null;
			for (int j = 0; j < rpg.Count; j++)
				found [((RecognizedPatternImage)rpg [j]).PCluster.Id] = -1;	
		}
	}
	private void fillRPGList<T>(int maxSize, PatternGroup group, List<int> indst1, List<T> rpi, int [] found, int [] grinds, int [] groups, List<RecognizedPatternGroup> rpglist) where T : RecognizedPattern {
		int matchedi = 0;
		groups [1] = 0;
		rpglist.Clear ();
		int[] curi = new int[group.Count];
		for (int i = 0; i < clusters.Count; i++) {
			found [i] = -1;
		}
		//TODO: for more than 2 members of the group optimize by checking all the pairs of clusters first 
		//(for each group 0 check which groups are admissible and then use only them to check groups 1..n)
		do {
			BoundingBox box = rpi [indst1 [0] + curi [0]].ClusterBox;
			int maxsizexi = maxSize, maxsizeyi = maxSize;
			for (int i = 1; i < matchedi; i++)
				box.Union (rpi [indst1 [i] + curi [i]].ClusterBox);
			bool isfound = true;
			for (int i = matchedi; i < group.Count && isfound; i++) {
				box.Union (rpi [indst1 [i] + curi [i]].ClusterBox);

				matchedi = i;
				if (box.W > maxsizexi || box.H > maxsizeyi) {
					isfound = false;
				}
				if (isfound && i == 0) {//TODO == 0
					float boxw = rpi [indst1 [0] + curi [0]].W, boxh = rpi [indst1 [0] + curi [0]].H, groupw = group.W, grouph = group.H;
					if ((boxw - boxh) * (group [0].W - group [0].H) < 0) {
						boxw = box.H;
						boxh = box.W;
						groupw = group.H;
						grouph = group.W; 
					}
					float boxscale = Mathf.Min (boxw / group [0].W, boxh / group [0].H);
					maxsizexi = Mathf.Min ((int)(groupw * boxscale / group.Tol * Mathf.Sqrt (2)), maxsizexi);
					maxsizeyi = Mathf.Min ((int)(grouph * boxscale / group.Tol * Mathf.Sqrt (2)), maxsizeyi);
				}
			}

			if (isfound) {
				float score = 0;
				for (int i = 0; i < group.Count; i++) {
					score += rpi [indst1 [i] + curi [i]].Score;
				}
				score /= group.Count;
				for (int i = 0; i < group.Count; i++) {
					RecognizedPattern rpii = rpi [indst1 [i] + curi [i]];
					if (grinds != null)
						grinds[i] = groups[1];
					if (rpii is RecognizedPatternImage) {
						int rpiiid = ((RecognizedPatternImage) rpii).PCluster.Id;
						if (found [rpiiid] >= 0 && rpglist [found [rpiiid]].Score < score) {
							score = -1;
							break;
						}
						groups[groups[1] + 2] = rpiiid + 1;
						groups[1]++;
					} else if (rpii is RecognizedPatternGroup) {
						RecognizedPatternGroup rpgi = (RecognizedPatternGroup) rpii;
						for (int j = 0; j < rpgi.Count; j++) {
							int rpiiid = ((RecognizedPatternImage) rpgi[j]).PCluster.Id;
							if (found [rpiiid] >= 0 && rpglist [found [rpiiid]].Score < score) {
								score = -1;
								break;
							}								
							groups[groups[1] + 2] = rpiiid + 1;
							groups[1]++;
						}
						if (score == -1)
							break;
					}
					if (grinds != null)
						grinds[group.Count] = groups[1];
				}
				if (score >= 0) {
					// check composition and set theta 
					int[] gbox = new int [4];
					float theta = checkGroup (group, grinds, groups, box, boundpoints_st, boundpoints_list, gbox);
					if (false)
						Debug.Log ("Score 2: " + score + ". Theta: " + theta);
					if (theta < 1000) {
						for (int i = 0; i < group.Count; i++) {
							RecognizedPattern rpii = rpi [indst1 [i] + curi [i]];
							if (rpii is RecognizedPatternImage) 
								eraseGroup(rpglist, found, (RecognizedPatternImage) rpii);
							else if (rpii is RecognizedPatternGroup) {
								RecognizedPatternGroup rpgi = (RecognizedPatternGroup) rpii;
								for (int j = 0; j < rpgi.Count; j++) 
									eraseGroup(rpglist, found, (RecognizedPatternImage) rpgi[j]);
							}									
						}
						RecognizedPatternGroup rpgnew = new RecognizedPatternGroup (group, theta, score, gbox [0], gbox [1], gbox [2], gbox [3]);
						for (int i = 0; i < group.Count; i++) {
							rpgnew [i] = rpi [indst1 [i] + curi [i]];
						}
						rpglist.Add (rpgnew);
						if (false)
							Debug.Log("Groups: " + groups[1] + "," + groups[2] + "," + groups[3] + "," + groups[4]);
						for (int j = 0; j < groups[1]; j++) {
							Cluster cluster = clusters[groups[j + 2] - 1];
							found[groups[j + 2] - 1] = rpglist.Count - 1;
							if (texture != null && grinds != null)
								drawRecognizedCluster (texture, cluster, score, score);
						}
					}
				}
			} else
				for (int i = matchedi + 1; i < group.Count; i++)
					curi [i] = 0;
			do {
				curi [matchedi]++;
				if (curi [matchedi] + indst1 [matchedi] < indst1 [matchedi + 1])
					break;
				curi [matchedi] = 0;
				matchedi--;
			} while (matchedi >= 0);
		} while (matchedi >= 0);	
	}

	public void RecognizeGroups(int minSize, int maxSize, List<PatternGroup> lowGroups, List<int> indst, List<RecognizedPatternGroup> all) {
		all.Clear ();
		indst.Clear ();
		List<int> indst1 = new List<int> (all.Capacity);
		List<RecognizedPatternImage> recognized1 = new List<RecognizedPatternImage> (all.Capacity);
		List<RecognizedPatternGroup> rpglist = new List<RecognizedPatternGroup> (all.Capacity);
		List<PatternImage> pats = new List<PatternImage> (all.Capacity);
		int[] found = new int [clusters.Count];
		int[] groups = new int[clusters.Count + 2];
		for (int k = 0; k < lowGroups.Count; k++) {
			PatternGroup group = lowGroups [k];
			pats.Clear ();
			for (int j = 0; j < group.Count; j++)
				pats.Add ((PatternImage)group [j]);
			RecognizePatterns (minSize, maxSize, pats, indst1, recognized1, null);
			for (int i = 0; i < group.Count; i++)
				if (indst1 [i] == indst1 [i + 1]) {
					pats.Clear ();
					break;
				}
			if (pats.Count == 0)
				continue;
			fillRPGList<RecognizedPatternImage> (maxSize, group, indst1, recognized1, found, null, groups, rpglist);

			indst.Add (all.Count);
			for (int i = 0; i < rpglist.Count; i++)
				if (rpglist [i] != null) {
					all.Add (rpglist [i]);
				}
		}
		indst.Add (all.Count);
	}


	/*private static PatternImage findFirstSubpatternImage(Pattern pattern) {
		while (pattern != null) {
			if (pattern is PatternImage)
				return (PatternImage)pattern;
			else if (pattern is PatternBaseGroup && ((PatternBaseGroup)pattern).Count > 0)
				pattern = ((PatternBaseGroup)pattern) [0];
			else
				break;
		}
		return null;
	}*/

	private static void drawPatternGroup (PatternGroup group, Texture2D texturePat, BoundingBox groupBox, int w, int h, float[] restorePattern) {
		for (int i = 0; i < group.Count; i++) {
			Pattern groupi = group [i];
			if (groupi is PatternClass)
				groupi = ((PatternClass)groupi) [0];
			float scaleX = groupBox.W / ((float)group.W);
			float scaleY = groupBox.H / ((float)group.H);
			BoundingBox box = group.GetBox (i);
			box.ScaleX (scaleX);
			box.ScaleY (scaleY);
			box.Shift (groupBox.MinX, groupBox.MinY);
	
			if (groupi is PatternImage) {
				PatternImage pattern = (PatternImage)groupi;
				RotateAndScale (pattern.Theta - group.Theta, box, 0, 0, w, h, restorePattern, pattern); //
				//Debug.Log ("Have patterns groups!: " + i + "," + pattern.W + "," + pattern.H + "," + box.MinX+","+box.MaxX + "," + box.MinY+","+box.MaxY + "," + pattern.Theta + ", " + group.Theta);
				/*for (int y = 0; y < pattern.H; y++)
						for (int x = 0; x < pattern.W; x++) {
							if (i == 0)
								texturePat.SetPixel (x, y, new Color (pattern.Pattern [y * pattern.W + x], pattern.Pattern [y * pattern.W + x], pattern.Pattern [y * pattern.W + x]));
							else
								texturePat.SetPixel (group.W - 1 - x, group.H - 1 - y, new Color (pattern.Pattern [y * pattern.W + x], pattern.Pattern [y * pattern.W + x], pattern.Pattern [y * pattern.W + x]));
											}*/
			} else if (groupi is PatternGroup) {
				drawPatternGroup (((PatternGroup)groupi), texturePat, box, w, h, restorePattern);
			}
			
		}
			//Debug.Log ("Have a group");
	}

	private static void drawPattern(float [] pattern, int w, int h, Texture2D texturePat) {
		float[] fxy = new float[(w + 2) * (h + 2)];
		float[] cls = new float[fxy.Length];
		float[] theta = new float[fxy.Length];
		int[] connect = new int[fxy.Length];
		fxy [0] = pattern [0];
		fxy [w + 1] = pattern [w - 1];
		fxy [(h + 1) * (w + 2)] = pattern [(h - 1) * w];
		fxy [(h + 2) * (w + 2) - 1] = pattern [h * w - 1];
		for (int y = 0; y < h; y++) {
			fxy [(y + 1) * (w + 2)] = pattern [y * w];
			fxy [(y + 2) * (w + 2) - 1] = pattern [(y + 1) * w - 1];
			for (int x = 0; x < w; x++) {
				fxy [x + 1] = pattern [x];
				fxy [(h + 1) * (w + 2) + x + 1] = pattern [(h - 1) * w + x];
			}
			for (int x = 0; x < w; x++) {
				fxy [(y + 1) * (w + 2) + x + 1] = pattern [y * w + x];
			}
		}
		Array.Copy (fxy, cls, fxy.Length);
		List<Cluster> clusters = new List<Cluster> (pattern.Length);

		FindEdge (fxy, cls, theta, 1, 1, w, h, w + 2);
		SuppressNonmax (cls, fxy, theta, 1, 1, w, h, w + 2);

		FindConnected (0.01f, 0.5f, fxy, connect, clusters, 1, 1, w, h, w + 2, h + 2);

		float minp = pattern[0], maxp = minp;
		for (int i = 1; i < pattern.Length; i++)
			if (pattern [i] < minp)
				minp = pattern [i];
			else if (pattern [i] > maxp)
				maxp = pattern [i];
		for (int y = 0; y < h; y++) 
			for (int x = 0; x < w; x++) 
				texturePat.SetPixel (x, y, new Color (0, 0, 0));//new Color (pattern [y * w + x], pattern [y * w + x], pattern [y * w + x]));//
		for (int y = 0; y < h; y++) {
			bool flag = false;
			bool nextflag = false;
			bool prevflag = false;
			for (int x = 0; x < w; x++) {
				/*if (connect [(y + 1) * (w + 2) + x + 1] != 0 && clusters[connect [(y + 1) * (w + 2) + x + 1]-1].Size > 20) {
					if (prevflag && nextflag) {
						flag = !flag;
						nextflag = false;
					} else
						nextflag = true;
					prevflag = true;
				} else {
					if (prevflag && !nextflag) {
						bool found = false;
						for (int x1 = x + 1; x1 < w; x1++)
							if (connect [(y + 1) * (w + 2) + x1 + 1] != 0) {
								found = true;
								break;
							}
						if (flag && !found)
							flag = false;
						if (!flag && found)
							flag = true;
					} 
					nextflag = false;
					prevflag = false;
				}*/
			
				//texturePat.SetPixel (x, y, new Color (fxy [(y+1) * (w+2) + x+1], fxy [(y+1) * (w+2) + x+1], fxy [(y+1) * (w+2) + x+1]));//
				if (connect [(y + 1) * (w + 2) + x + 1] != 0) { //pattern [y * w + x] > minp+(maxp - minp) * 0.5)
					texturePat.SetPixel (x, y, new Color (1f, 1f, 0f));
					//texturePat.SetPixel (x, y+1, new Color (1f, 1f, 0f));
				}
				//else
				//	texturePat.SetPixel (x, y, new Color (0, 0, 0));//new Color (pattern [y * w + x], pattern [y * w + x], pattern [y * w + x]));//
				//if (nextflag)
				//	flag = !flag;
			}
		}
	}
	public static void DrawPattern(Pattern lastPattern, Texture2D texturePat) {
		if (lastPattern is PatternClass && ((PatternClass)lastPattern).Count > 0)
			lastPattern = ((PatternClass)lastPattern) [0];
		if (lastPattern is PatternGroup) {
			PatternGroup group = (PatternGroup)lastPattern;
			float[] restorePattern = new float[group.W * group.H];
			for (int i = 0; i < restorePattern.Length; i++)
				restorePattern [i] = 1f;
			drawPatternGroup (group, texturePat, new BoundingBox(0, 0, group.W, group.H), group.W, group.H, restorePattern);
			for (int y = 0; y < group.H; y++)
				for (int x = 0; x < group.W; x++) {
					texturePat.SetPixel (x, y, new Color (restorePattern [y * group.W + x], restorePattern [y * group.W + x], restorePattern [y * group.W + x]));
				}
			drawPattern(restorePattern, group.W, group.H, texturePat);
			for (int i = 0; i < group.Count; i++)
				drawRecognizedCluster (texturePat, group.GetBox(i), 0.0f, 0.0f);
		} else if (lastPattern is PatternImage) {//manager.PatternsToRecognize.Count > 0) {
			PatternImage pattern = (PatternImage)lastPattern;// manager.PatternsToRecognize [manager.PatternsToRecognize.Count - 1];
			//Debug.Log ("Have patterns!: " + pattern.H + ", " + pattern.W);
			drawPattern(pattern.Pattern, pattern.W, pattern.H, texturePat);
		}
	}

	private RecognizedPattern[] getBestMatches(List<RecognizedPatternImage> patterns, List<RecognizedPatternGroup> lowLevelGroups) {
		RecognizedPattern[] bestMatches = new RecognizedPattern[clusters.Count];
		for (int i = 0; i < lowLevelGroups.Count; i++) {
			RecognizedPatternGroup pg = lowLevelGroups [i];
			for (int j = 0; j < pg.Count; j++) {
				RecognizedPatternImage pgi = (RecognizedPatternImage)pg [j];
				if (bestMatches [pgi.PCluster.Id] == null || bestMatches [pgi.PCluster.Id].Score > pg.Score) {
					if (bestMatches [pgi.PCluster.Id] != null) {
						RecognizedPatternGroup rpg = (RecognizedPatternGroup)bestMatches [pgi.PCluster.Id];
						for (int k = 0; k < rpg.Count; k++) {
							bestMatches [((RecognizedPatternImage)rpg [i]).PCluster.Id] = null;
						}
					}
					bestMatches [pgi.PCluster.Id] = pg;
				}
			}
		}
		for (int i = 0; i < patterns.Count; i++)
			if (bestMatches [patterns [i].PCluster.Id] == null || (!(bestMatches [patterns [i].PCluster.Id] is RecognizedPatternGroup) &&
				bestMatches [patterns [i].PCluster.Id].Score > patterns [i].Score)) {
				bestMatches [patterns [i].PCluster.Id] = patterns [i];
			}
		return bestMatches;
	}

	public Pattern FindPatternToRecognize(int captSize, List<int> indpi, List<RecognizedPatternImage> patterns, List<int> indpg, List<RecognizedPatternGroup> lowLevelGroups, int patternid, bool types)  {
		RecognizedPattern[] bestMatches = getBestMatches (patterns, lowLevelGroups);
		int minDist = w + h, mini = -1;
		for (int i = 0; i < clusters.Count; i++)
			if (CheckCluster(i) && bestMatches [i] != null) {
				int dist1 = dist (clusters [i], w / 2, h / 2);
				if (dist1 < minDist) {
					minDist = dist1;
					mini = i;
				}
			}
		if (mini < 0)
			return null;
		BoundingBox box = new BoundingBox (clusters [mini]);
		int[] groups = new int[clusters.Count + 2];
		groups [2] = mini + 1;
		int glen = 1;
		for (int i = 0; i < clusters.Count; i++)
			if (CheckCluster(i) && i != mini && bestMatches[i] != null && maxsize (box, clusters [i]) < captSize) {
				groups [glen + 2] = i + 1;
				glen++;
				box.Union (clusters [i]);
			}
		groups[1] = glen;
		if (glen == 1)
			return types ? bestMatches [mini].Pattern.Parent : bestMatches [mini].Pattern;
		
		int[] newbox = new int[4]; 
		int[] newgroupbox = new int[4];
		float thetaturn = fixTheta (findTheta (groups, box.MinX, box.MinY, boundpoints_st, boundpoints_list), Mathf.PI / 4);

		calc_new_box (-thetaturn, box.MinX, box.MinY, groups, boundpoints_st, boundpoints_list, newgroupbox);
		
		PatternGroup group = new PatternGroup (patternid);
		group.Theta = thetaturn;
		int[] gr = new int[groups [1] + 2];
		for (int i = 0; i < groups [1]; i++)
			if (groups [2 + i] > 0) {
				Pattern patternToAdd = bestMatches [groups [2 + i] - 1].Pattern;
				gr [1] = 1;
				gr [2] = groups [2 + i];
				if (patternToAdd is PatternGroup) {
					for (int j = i + 1; j < groups [1]; j++) 
						if (groups [2 + j] > 0 && bestMatches [groups [2 + j] - 1].Pattern.Id == patternToAdd.Id) {//TODO: not very effective?
							gr [2 + gr[1]] = groups[2 + j];
							gr [1]++;
							groups [2 + j] = -groups [ 2 + j];
						}
				}
				calc_new_box (-thetaturn, box.MinX, box.MinY, gr, boundpoints_st, boundpoints_list, newbox);
				if (types)
					patternToAdd = patternToAdd.Parent;
				group.add_pattern (new BoundingBox(newbox[0] - newgroupbox[0], newbox[1] - newgroupbox[1], newbox[2] - newgroupbox[0],
					newbox[3] - newgroupbox[1]), patternToAdd);
			}
		if (groups [1] > 0) {
			for (int i = 0; i < groups [1]; i++) 
				if (groups[i + 2] != 0) {
					int groupb = Mathf.Abs(groups [i + 2]);
					Cluster cluster = clusters [groupb - 1];
					for (int x = cluster.MinX; x <= cluster.MaxX; x++) {
						texture.SetPixel (x, cluster.MinY, new Color (0.0f, 1f, 1f));
						texture.SetPixel (x, cluster.MaxY, new Color (0.0f, 1f, 1f));
					}
					for (int y = cluster.MinY; y <= cluster.MaxY; y++) {
						texture.SetPixel (cluster.MinX, y, new Color (0.0f, 1f, 1f));
						texture.SetPixel (cluster.MaxX, y, new Color (0.0f, 1f, 1f));
					}
				}
		}

		return group;
	}
	public void RecognizeAll(int maxSize, List<int> indpi, List<RecognizedPatternImage> patterns, List<int> indpg, List<RecognizedPatternGroup> lowLevelGroups, List<Pattern> patternsToRecognize, List<int> indst, List<RecognizedPattern> all)  {
		//RecognizedPattern[] bestMatches = getBestMatches (patterns, lowLevelGroups);
		all.Clear ();
		indst.Clear ();
		List<RecognizedPattern> rpi = new List<RecognizedPattern> (all.Capacity);
		List<int> indst1 = new List<int> (all.Capacity);
		List<RecognizedPatternGroup> rpglist = new List<RecognizedPatternGroup> (all.Capacity);
		int[] foundp = new int [clusters.Count];
		int[] foundpg = new int [clusters.Count];
		int[] found = new int [clusters.Count];
		int[] groups = new int[clusters.Count + 2];
		int[] grinds = new int[clusters.Count + 1];
		for (int i = 0; i < clusters.Count; i++) {
			foundp [i] = -1;
			foundpg [i] = -1;
		}
		for (int k = 0; k < patternsToRecognize.Count; k++) {
			indst.Add (all.Count);
			if (patternsToRecognize [k] is PatternImage || patternsToRecognize [k] is PatternClass) {
				for (int i = 0; i < patterns.Count; i++)
					if (patterns [i].Pattern.Id == patternsToRecognize [k].Id || patterns [i].Pattern.Parent.Id == patternsToRecognize [k].Id)
					if (foundp [patterns [i].PCluster.Id] < 0 || patterns [foundp [patterns [i].PCluster.Id]].Score > patterns [i].Score)
						foundp [patterns [i].PCluster.Id] = i;
				for (int i = 0; i < clusters.Count; i++)
					if (CheckCluster(i) && foundp [i] >= 0) {
						all.Add (patterns [foundp [i]]);
						foundp [i] = -1;
					}
			} 
			if ((patternsToRecognize [k] is PatternGroup && ((PatternGroup)patternsToRecognize [k]) [0] is PatternImage)  
				|| patternsToRecognize [k] is PatternClass) {
				for (int i = 0; i < lowLevelGroups.Count; i++)
					if (lowLevelGroups [i].Pattern.Id == patternsToRecognize [k].Id || patterns [i].Pattern.Parent.Id == patternsToRecognize [k].Id) {
						bool replace = true;
						for (int j = 0; j < lowLevelGroups [i].Count && replace; j++) {
							RecognizedPatternImage pi = (RecognizedPatternImage)(lowLevelGroups [i] [j]);
							if (foundpg [pi.PCluster.Id] >= 0 && lowLevelGroups [foundpg [pi.PCluster.Id]].Score < pi.Score)
								replace = false;
						}
						if (replace) 
							for (int j = 0; j < lowLevelGroups [i].Count && replace; j++) {
								RecognizedPatternImage pi = (RecognizedPatternImage)(lowLevelGroups [i] [j]);
								if (foundpg [pi.PCluster.Id] >= 0) {
									int llgid = foundpg [pi.PCluster.Id];
									for (int j1 = 0; j1 < lowLevelGroups [llgid].Count; j1++)
										foundpg [((RecognizedPatternImage)(lowLevelGroups [llgid] [j1])).PCluster.Id] = -1;
								}
								foundpg [pi.PCluster.Id] = i;
							}
					}
				for (int i = 0; i < clusters.Count; i++)
					if (CheckCluster(i) && foundpg [i] >= 0) {
						all.Add (lowLevelGroups [foundpg [i]]);
						foundpg [i] = -1;
					}
			} else if (patternsToRecognize [k] is PatternGroup) {
				PatternGroup group = (PatternGroup)patternsToRecognize [k];
				rpi.Clear ();
				indst1.Clear ();
				for (int k1 = 0; k1 < group.Count; k1++) {
					indst1.Add (k1);
					if (group [k1] is PatternImage || group [k1] is PatternClass)
						for (int i = 0; i < patterns.Count; i++)
							if (patterns [i].Pattern.Id == group [k1].Id || patterns [i].Pattern.Parent.Id == group [k1].Id)
								rpi.Add (patterns [i]);
					if ((group [k1] is PatternGroup && ((PatternGroup)group [k1]) [0] is PatternImage)  
						|| group [k1] is PatternClass) 
						for (int i = 0; i < lowLevelGroups.Count; i++)
							if (lowLevelGroups [i].Pattern.Id == group [k1].Id || lowLevelGroups [i].Pattern.Parent.Id == group [k1].Id) {
								rpi.Add (lowLevelGroups [i]);
							}
				}
				indst1.Add(rpi.Count);
				for (int i = 0; i < group.Count; i++)
					if (indst1 [i] == indst1 [i + 1]) {
						rpi.Clear ();
						break;
					}
				if (rpi.Count == 0)
					continue;
				
				fillRPGList<RecognizedPattern> (maxSize, group, indst1, rpi, found, grinds, groups, rpglist);
				for (int i = 0; i < rpglist.Count; i++)
					if (rpglist [i] != null) {
						all.Add (rpglist [i]);
					}
			}
		}
		indst.Add (all.Count);	
	}
}

