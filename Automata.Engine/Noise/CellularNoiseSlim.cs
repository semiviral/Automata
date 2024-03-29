using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Automata.Engine.Noise
{
    public static class CellularNoiseSlim
    {
        public enum CellularDistanceFunction
        {
            Euclidean,
            Manhattan,
            Natural
        }

        public enum CellularReturnType
        {
            CellValue,
            NoiseLookup,
            Distance,
            Distance2,
            Distance2Add,
            Distance2Sub,
            Distance2Mul,
            Distance2Div
        }

        private const int _X_PRIME = 1619;
        private const int _Y_PRIME = 31337;
        private const int _Z_PRIME = 6971;

        private static ReadOnlySpan<Vector3> Cell3D => new[]
        {
            new Vector3(-0.7292736885f, -0.6618439697f, 0.1735581948f),
            new Vector3(0.790292081f, -0.5480887466f, -0.2739291014f),
            new Vector3(0.7217578935f, 0.6226212466f, -0.3023380997f),
            new Vector3(0.565683137f, -0.8208298145f, -0.0790000257f),
            new Vector3(0.760049034f, -0.5555979497f, -0.3370999617f),
            new Vector3(0.3713945616f, 0.5011264475f, 0.7816254623f),
            new Vector3(-0.1277062463f, -0.4254438999f, -0.8959289049f),
            new Vector3(-0.2881560924f, -0.5815838982f, 0.7607405838f),
            new Vector3(0.5849561111f, -0.662820239f, -0.4674352136f),
            new Vector3(0.3307171178f, 0.0391653737f, 0.94291689f),
            new Vector3(0.8712121778f, -0.4113374369f, -0.2679381538f),
            new Vector3(0.580981015f, 0.7021915846f, 0.4115677815f),
            new Vector3(0.503756873f, 0.6330056931f, -0.5878203852f),
            new Vector3(0.4493712205f, 0.601390195f, 0.6606022552f),
            new Vector3(-0.6878403724f, 0.09018890807f, -0.7202371714f),
            new Vector3(-0.5958956522f, -0.6469350577f, 0.475797649f),
            new Vector3(-0.5127052122f, 0.1946921978f, -0.8361987284f),
            new Vector3(-0.9911507142f, -0.05410276466f, -0.1212153153f),
            new Vector3(-0.2149721042f, 0.9720882117f, -0.09397607749f),
            new Vector3(-0.7518650936f, -0.5428057603f, 0.3742469607f),
            new Vector3(0.5237068895f, 0.8516377189f, -0.02107817834f),
            new Vector3(0.6333504779f, 0.1926167129f, -0.7495104896f),
            new Vector3(-0.06788241606f, 0.3998305789f, 0.9140719259f),
            new Vector3(-0.5538628599f, -0.4729896695f, -0.6852128902f),
            new Vector3(-0.7261455366f, -0.5911990757f, 0.3509933228f),
            new Vector3(-0.9229274737f, -0.1782808786f, 0.3412049336f),
            new Vector3(-0.6968815002f, 0.6511274338f, 0.3006480328f),
            new Vector3(0.9608044783f, -0.2098363234f, -0.1811724921f),
            new Vector3(0.06817146062f, -0.9743405129f, 0.2145069156f),
            new Vector3(-0.3577285196f, -0.6697087264f, -0.6507845481f),
            new Vector3(-0.1868621131f, 0.7648617052f, -0.6164974636f),
            new Vector3(-0.6541697588f, 0.3967914832f, 0.6439087246f),
            new Vector3(0.6993340405f, -0.6164538506f, 0.3618239211f),
            new Vector3(-0.1546665739f, 0.6291283928f, 0.7617583057f),
            new Vector3(-0.6841612949f, -0.2580482182f, -0.6821542638f),
            new Vector3(0.5383980957f, 0.4258654885f, 0.7271630328f),
            new Vector3(-0.5026987823f, -0.7939832935f, -0.3418836993f),
            new Vector3(0.3202971715f, 0.2834415347f, 0.9039195862f),
            new Vector3(0.8683227101f, -0.0003762656404f, -0.4959995258f),
            new Vector3(0.791120031f, -0.08511045745f, 0.6057105799f),
            new Vector3(-0.04011016052f, -0.4397248749f, 0.8972364289f),
            new Vector3(0.9145119872f, 0.3579346169f, -0.1885487608f),
            new Vector3(-0.9612039066f, -0.2756484276f, 0.01024666929f),
            new Vector3(0.6510361721f, -0.2877799159f, -0.7023778346f),
            new Vector3(-0.2041786351f, 0.7365237271f, 0.644859585f),
            new Vector3(-0.7718263711f, 0.3790626912f, 0.5104855816f),
            new Vector3(-0.3060082741f, -0.7692987727f, 0.5608371729f),
            new Vector3(0.454007341f, -0.5024843065f, 0.7357899537f),
            new Vector3(0.4816795475f, 0.6021208291f, -0.6367380315f),
            new Vector3(0.6961980369f, -0.3222197429f, 0.641469197f),
            new Vector3(-0.6532160499f, -0.6781148932f, 0.3368515753f),
            new Vector3(0.5089301236f, -0.6154662304f, -0.6018234363f),
            new Vector3(-0.1635919754f, -0.9133604627f, -0.372840892f),
            new Vector3(0.52408019f, -0.8437664109f, 0.1157505864f),
            new Vector3(0.5902587356f, 0.4983817807f, -0.6349883666f),
            new Vector3(0.5863227872f, 0.494764745f, 0.6414307729f),
            new Vector3(0.6779335087f, 0.2341345225f, 0.6968408593f),
            new Vector3(0.7177054546f, -0.6858979348f, 0.120178631f),
            new Vector3(-0.5328819713f, -0.5205125012f, 0.6671608058f),
            new Vector3(-0.8654874251f, -0.0700727088f, -0.4960053754f),
            new Vector3(-0.2861810166f, 0.7952089234f, 0.5345495242f),
            new Vector3(-0.04849529634f, 0.9810836427f, -0.1874115585f),
            new Vector3(-0.6358521667f, 0.6058348682f, 0.4781800233f),
            new Vector3(0.6254794696f, -0.2861619734f, 0.7258696564f),
            new Vector3(-0.2585259868f, 0.5061949264f, -0.8227581726f),
            new Vector3(0.02136306781f, 0.5064016808f, -0.8620330371f),
            new Vector3(0.200111773f, 0.8599263484f, 0.4695550591f),
            new Vector3(0.4743561372f, 0.6014985084f, -0.6427953014f),
            new Vector3(0.6622993731f, -0.5202474575f, -0.5391679918f),
            new Vector3(0.08084972818f, -0.6532720452f, 0.7527940996f),
            new Vector3(-0.6893687501f, 0.0592860349f, 0.7219805347f),
            new Vector3(-0.1121887082f, -0.9673185067f, 0.2273952515f),
            new Vector3(0.7344116094f, 0.5979668656f, -0.3210532909f),
            new Vector3(0.5789393465f, -0.2488849713f, 0.7764570201f),
            new Vector3(0.6988182827f, 0.3557169806f, -0.6205791146f),
            new Vector3(-0.8636845529f, -0.2748771249f, -0.4224826141f),
            new Vector3(-0.4247027957f, -0.4640880967f, 0.777335046f),
            new Vector3(0.5257722489f, -0.8427017621f, 0.1158329937f),
            new Vector3(0.9343830603f, 0.316302472f, -0.1639543925f),
            new Vector3(-0.1016836419f, -0.8057303073f, -0.5834887393f),
            new Vector3(-0.6529238969f, 0.50602126f, -0.5635892736f),
            new Vector3(-0.2465286165f, -0.9668205684f, -0.06694497494f),
            new Vector3(-0.9776897119f, -0.2099250524f, -0.007368825344f),
            new Vector3(0.7736893337f, 0.5734244712f, 0.2694238123f),
            new Vector3(-0.6095087895f, 0.4995678998f, 0.6155736747f),
            new Vector3(0.5794535482f, 0.7434546771f, 0.3339292269f),
            new Vector3(-0.8226211154f, 0.08142581855f, 0.5627293636f),
            new Vector3(-0.510385483f, 0.4703667658f, 0.7199039967f),
            new Vector3(-0.5764971849f, -0.07231656274f, -0.8138926898f),
            new Vector3(0.7250628871f, 0.3949971505f, -0.5641463116f),
            new Vector3(-0.1525424005f, 0.4860840828f, -0.8604958341f),
            new Vector3(-0.5550976208f, -0.4957820792f, 0.667882296f),
            new Vector3(-0.1883614327f, 0.9145869398f, 0.357841725f),
            new Vector3(0.7625556724f, -0.5414408243f, -0.3540489801f),
            new Vector3(-0.5870231946f, -0.3226498013f, -0.7424963803f),
            new Vector3(0.3051124198f, 0.2262544068f, -0.9250488391f),
            new Vector3(0.6379576059f, 0.577242424f, -0.5097070502f),
            new Vector3(-0.5966775796f, 0.1454852398f, -0.7891830656f),
            new Vector3(-0.658330573f, 0.6555487542f, -0.3699414651f),
            new Vector3(0.7434892426f, 0.2351084581f, 0.6260573129f),
            new Vector3(0.5562114096f, 0.8264360377f, -0.0873632843f),
            new Vector3(-0.3028940016f, -0.8251527185f, 0.4768419182f),
            new Vector3(0.1129343818f, -0.985888439f, -0.1235710781f),
            new Vector3(0.5937652891f, -0.5896813806f, 0.5474656618f),
            new Vector3(0.6757964092f, -0.5835758614f, -0.4502648413f),
            new Vector3(0.7242302609f, -0.1152719764f, 0.6798550586f),
            new Vector3(-0.9511914166f, 0.0753623979f, -0.2992580792f),
            new Vector3(0.2539470961f, -0.1886339355f, 0.9486454084f),
            new Vector3(0.571433621f, -0.1679450851f, -0.8032795685f),
            new Vector3(-0.06778234979f, 0.3978269256f, 0.9149531629f),
            new Vector3(0.6074972649f, 0.733060024f, -0.3058922593f),
            new Vector3(-0.5435478392f, 0.1675822484f, 0.8224791405f),
            new Vector3(-0.5876678086f, -0.3380045064f, -0.7351186982f),
            new Vector3(-0.7967562402f, 0.04097822706f, -0.6029098428f),
            new Vector3(-0.1996350917f, 0.8706294745f, 0.4496111079f),
            new Vector3(-0.02787660336f, -0.9106232682f, -0.4122962022f),
            new Vector3(-0.7797625996f, -0.6257634692f, 0.01975775581f),
            new Vector3(-0.5211232846f, 0.7401644346f, -0.4249554471f),
            new Vector3(0.8575424857f, 0.4053272873f, -0.3167501783f),
            new Vector3(0.1045223322f, 0.8390195772f, -0.5339674439f),
            new Vector3(0.3501822831f, 0.9242524096f, -0.1520850155f),
            new Vector3(0.1987849858f, 0.07647613266f, 0.9770547224f),
            new Vector3(0.7845996363f, 0.6066256811f, -0.1280964233f),
            new Vector3(0.09006737436f, -0.9750989929f, -0.2026569073f),
            new Vector3(-0.8274343547f, -0.542299559f, 0.1458203587f),
            new Vector3(-0.3485797732f, -0.415802277f, 0.840000362f),
            new Vector3(-0.2471778936f, -0.7304819962f, -0.6366310879f),
            new Vector3(-0.3700154943f, 0.8577948156f, 0.3567584454f),
            new Vector3(0.5913394901f, -0.548311967f, -0.5913303597f),
            new Vector3(0.1204873514f, -0.7626472379f, -0.6354935001f),
            new Vector3(0.616959265f, 0.03079647928f, 0.7863922953f),
            new Vector3(0.1258156836f, -0.6640829889f, -0.7369967419f),
            new Vector3(-0.6477565124f, -0.1740147258f, -0.7417077429f),
            new Vector3(0.6217889313f, -0.7804430448f, -0.06547655076f),
            new Vector3(0.6589943422f, -0.6096987708f, 0.4404473475f),
            new Vector3(-0.2689837504f, -0.6732403169f, -0.6887635427f),
            new Vector3(-0.3849775103f, 0.5676542638f, 0.7277093879f),
            new Vector3(0.5754444408f, 0.8110471154f, -0.1051963504f),
            new Vector3(0.9141593684f, 0.3832947817f, 0.131900567f),
            new Vector3(-0.107925319f, 0.9245493968f, 0.3654593525f),
            new Vector3(0.377977089f, 0.3043148782f, 0.8743716458f),
            new Vector3(-0.2142885215f, -0.8259286236f, 0.5214617324f),
            new Vector3(0.5802544474f, 0.4148098596f, -0.7008834116f),
            new Vector3(-0.1982660881f, 0.8567161266f, -0.4761596756f),
            new Vector3(-0.03381553704f, 0.3773180787f, -0.9254661404f),
            new Vector3(-0.6867922841f, -0.6656597827f, 0.2919133642f),
            new Vector3(0.7731742607f, -0.2875793547f, -0.5652430251f),
            new Vector3(-0.09655941928f, 0.9193708367f, -0.3813575004f),
            new Vector3(0.2715702457f, -0.9577909544f, -0.09426605581f),
            new Vector3(0.2451015704f, -0.6917998565f, -0.6792188003f),
            new Vector3(0.977700782f, -0.1753855374f, 0.1155036542f),
            new Vector3(-0.5224739938f, 0.8521606816f, 0.02903615945f),
            new Vector3(-0.7734880599f, -0.5261292347f, 0.3534179531f),
            new Vector3(-0.7134492443f, -0.269547243f, 0.6467878011f),
            new Vector3(0.1644037271f, 0.5105846203f, -0.8439637196f),
            new Vector3(0.6494635788f, 0.05585611296f, 0.7583384168f),
            new Vector3(-0.4711970882f, 0.5017280509f, -0.7254255765f),
            new Vector3(-0.6335764307f, -0.2381686273f, -0.7361091029f),
            new Vector3(-0.9021533097f, -0.270947803f, -0.3357181763f),
            new Vector3(-0.3793711033f, 0.872258117f, 0.3086152025f),
            new Vector3(-0.6855598966f, -0.3250143309f, 0.6514394162f),
            new Vector3(0.2900942212f, -0.7799057743f, -0.5546100667f),
            new Vector3(-0.2098319339f, 0.85037073f, 0.4825351604f),
            new Vector3(-0.4592603758f, 0.6598504336f, -0.5947077538f),
            new Vector3(0.8715945488f, 0.09616365406f, -0.4807031248f),
            new Vector3(-0.6776666319f, 0.7118504878f, -0.1844907016f),
            new Vector3(0.7044377633f, 0.312427597f, 0.637304036f),
            new Vector3(-0.7052318886f, -0.2401093292f, -0.6670798253f),
            new Vector3(0.081921007f, -0.7207336136f, -0.6883545647f),
            new Vector3(-0.6993680906f, -0.5875763221f, -0.4069869034f),
            new Vector3(-0.1281454481f, 0.6419895885f, 0.7559286424f),
            new Vector3(-0.6337388239f, -0.6785471501f, -0.3714146849f),
            new Vector3(0.5565051903f, -0.2168887573f, -0.8020356851f),
            new Vector3(-0.5791554484f, 0.7244372011f, -0.3738578718f),
            new Vector3(0.1175779076f, -0.7096451073f, 0.6946792478f),
            new Vector3(-0.6134619607f, 0.1323631078f, 0.7785527795f),
            new Vector3(0.6984635305f, -0.02980516237f, -0.715024719f),
            new Vector3(0.8318082963f, -0.3930171956f, 0.3919597455f),
            new Vector3(0.1469576422f, 0.05541651717f, -0.9875892167f),
            new Vector3(0.708868575f, -0.2690503865f, 0.6520101478f),
            new Vector3(0.2726053183f, 0.67369766f, -0.68688995f),
            new Vector3(-0.6591295371f, 0.3035458599f, -0.6880466294f),
            new Vector3(0.4815131379f, -0.7528270071f, 0.4487723203f),
            new Vector3(0.9430009463f, 0.1675647412f, -0.2875261255f),
            new Vector3(0.434802957f, 0.7695304522f, -0.4677277752f),
            new Vector3(0.3931996188f, 0.594473625f, 0.7014236729f),
            new Vector3(0.7254336655f, -0.603925654f, 0.3301814672f),
            new Vector3(0.7590235227f, -0.6506083235f, 0.02433313207f),
            new Vector3(-0.8552768592f, -0.3430042733f, 0.3883935666f),
            new Vector3(-0.6139746835f, 0.6981725247f, 0.3682257648f),
            new Vector3(-0.7465905486f, -0.5752009504f, 0.3342849376f),
            new Vector3(0.5730065677f, 0.810555537f, -0.1210916791f),
            new Vector3(-0.9225877367f, -0.3475211012f, -0.167514036f),
            new Vector3(-0.7105816789f, -0.4719692027f, -0.5218416899f),
            new Vector3(-0.08564609717f, 0.3583001386f, 0.929669703f),
            new Vector3(-0.8279697606f, -0.2043157126f, 0.5222271202f),
            new Vector3(0.427944023f, 0.278165994f, 0.8599346446f),
            new Vector3(0.5399079671f, -0.7857120652f, -0.3019204161f),
            new Vector3(0.5678404253f, -0.5495413974f, -0.6128307303f),
            new Vector3(-0.9896071041f, 0.1365639107f, -0.04503418428f),
            new Vector3(-0.6154342638f, -0.6440875597f, 0.4543037336f),
            new Vector3(0.1074204368f, -0.7946340692f, 0.5975094525f),
            new Vector3(-0.3595449969f, -0.8885529948f, 0.28495784f),
            new Vector3(-0.2180405296f, 0.1529888965f, 0.9638738118f),
            new Vector3(-0.7277432317f, -0.6164050508f, -0.3007234646f),
            new Vector3(0.7249729114f, -0.00669719484f, 0.6887448187f),
            new Vector3(-0.5553659455f, -0.5336586252f, 0.6377908264f),
            new Vector3(0.5137558015f, 0.7976208196f, -0.3160000073f),
            new Vector3(-0.3794024848f, 0.9245608561f, -0.03522751494f),
            new Vector3(0.8229248658f, 0.2745365933f, -0.4974176556f),
            new Vector3(-0.5404114394f, 0.6091141441f, 0.5804613989f),
            new Vector3(0.8036581901f, -0.2703029469f, 0.5301601931f),
            new Vector3(0.6044318879f, 0.6832968393f, 0.4095943388f),
            new Vector3(0.06389988817f, 0.9658208605f, -0.2512108074f),
            new Vector3(0.1087113286f, 0.7402471173f, -0.6634877936f),
            new Vector3(-0.713427712f, -0.6926784018f, 0.1059128479f),
            new Vector3(0.6458897819f, -0.5724548511f, -0.5050958653f),
            new Vector3(-0.6553931414f, 0.7381471625f, 0.159995615f),
            new Vector3(0.3910961323f, 0.9188871375f, -0.05186755998f),
            new Vector3(-0.4879022471f, -0.5904376907f, 0.6429111375f),
            new Vector3(0.6014790094f, 0.7707441366f, -0.2101820095f),
            new Vector3(-0.5677173047f, 0.7511360995f, 0.3368851762f),
            new Vector3(0.7858573506f, 0.226674665f, 0.5753666838f),
            new Vector3(-0.4520345543f, -0.604222686f, -0.6561857263f),
            new Vector3(0.002272116345f, 0.4132844051f, -0.9105991643f),
            new Vector3(-0.5815751419f, -0.5162925989f, 0.6286591339f),
            new Vector3(-0.03703704785f, 0.8273785755f, 0.5604221175f),
            new Vector3(-0.5119692504f, 0.7953543429f, -0.3244980058f),
            new Vector3(-0.2682417366f, -0.9572290247f, -0.1084387619f),
            new Vector3(-0.2322482736f, -0.9679131102f, -0.09594243324f),
            new Vector3(0.3554328906f, -0.8881505545f, 0.2913006227f),
            new Vector3(0.7346520519f, -0.4371373164f, 0.5188422971f),
            new Vector3(0.9985120116f, 0.04659011161f, -0.02833944577f),
            new Vector3(-0.3727687496f, -0.9082481361f, 0.1900757285f),
            new Vector3(0.91737377f, -0.3483642108f, 0.1925298489f),
            new Vector3(0.2714911074f, 0.4147529736f, -0.8684886582f),
            new Vector3(0.5131763485f, -0.7116334161f, 0.4798207128f),
            new Vector3(-0.8737353606f, 0.18886992f, -0.4482350644f),
            new Vector3(0.8460043821f, -0.3725217914f, 0.3814499973f),
            new Vector3(0.8978727456f, -0.1780209141f, -0.4026575304f),
            new Vector3(0.2178065647f, -0.9698322841f, -0.1094789531f),
            new Vector3(-0.1518031304f, -0.7788918132f, -0.6085091231f),
            new Vector3(-0.2600384876f, -0.4755398075f, -0.8403819825f),
            new Vector3(0.572313509f, -0.7474340931f, -0.3373418503f),
            new Vector3(-0.7174141009f, 0.1699017182f, -0.6756111411f),
            new Vector3(-0.684180784f, 0.02145707593f, -0.7289967412f),
            new Vector3(-0.2007447902f, 0.06555605789f, -0.9774476623f),
            new Vector3(-0.1148803697f, -0.8044887315f, 0.5827524187f),
            new Vector3(-0.7870349638f, 0.03447489231f, 0.6159443543f),
            new Vector3(-0.2015596421f, 0.6859872284f, 0.6991389226f),
            new Vector3(-0.08581082512f, -0.10920836f, -0.9903080513f),
            new Vector3(0.5532693395f, 0.7325250401f, -0.396610771f),
            new Vector3(-0.1842489331f, -0.9777375055f, -0.1004076743f),
            new Vector3(0.0775473789f, -0.9111505856f, 0.4047110257f),
            new Vector3(0.1399838409f, 0.7601631212f, -0.6344734459f),
            new Vector3(0.4484419361f, -0.845289248f, 0.2904925424f)
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FastRound(float f) => f >= 0 ? (int)(f + 0.5f) : (int)(f - 0.5f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Hash3D(int seed, int x, int y, int z)
        {
            int hash = seed;
            hash ^= _X_PRIME * x;
            hash ^= _Y_PRIME * y;
            hash ^= _Z_PRIME * z;

            hash = hash * hash * hash * 60493;
            hash = (hash >> 13) ^ hash;

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float ValCoord3D(int seed, int x, int y, int z)
        {
            int n = seed;
            n ^= _X_PRIME * x;
            n ^= _Y_PRIME * y;
            n ^= _Z_PRIME * z;

            return (n * n * n * 60493f) / 2147483648f;
        }

        public static float GetCellular(int seed, CellularDistanceFunction distanceFunction, CellularReturnType returnType, float frequency,
            float jitter, Vector3 xyz) => SingleCellular(seed, distanceFunction, returnType, frequency, jitter, xyz.X, xyz.Y, xyz.Z);

        private static float SingleCellular(int seed, CellularDistanceFunction distanceFunction, CellularReturnType returnType, float frequency,
            float jitter, float x, float y, float z)
        {
            int xr = FastRound(x * frequency);
            int yr = FastRound(y * frequency);
            int zr = FastRound(z * frequency);

            float distance = 999999f;
            int xc = 0, yc = 0, zc = 0;

            switch (distanceFunction)
            {
                case CellularDistanceFunction.Euclidean:
                    for (int xi = xr - 1; xi <= (xr + 1); xi++)
                    for (int yi = yr - 1; yi <= (yr + 1); yi++)
                    for (int zi = zr - 1; zi <= (zr + 1); zi++)
                    {
                        Vector3 vec = Cell3D[Hash3D(seed, xi, yi, zi) & 255];

                        float vec_x = (xi - x) + (vec.X * jitter);
                        float vec_y = (yi - y) + (vec.Y * jitter);
                        float vec_z = (zi - z) + (vec.Z * jitter);

                        float new_distance = (vec_x * vec_x) + (vec_y * vec_y) + (vec_z * vec_z);

                        if (new_distance < distance)
                        {
                            distance = new_distance;
                            xc = xi;
                            yc = yi;
                            zc = zi;
                        }
                    }

                    break;
                case CellularDistanceFunction.Manhattan:
                    for (int xi = xr - 1; xi <= (xr + 1); xi++)
                    for (int yi = yr - 1; yi <= (yr + 1); yi++)
                    for (int zi = zr - 1; zi <= (zr + 1); zi++)
                    {
                        Vector3 vec = Cell3D[Hash3D(seed, xi, yi, zi) & 255];

                        float vec_x = (xi - x) + (vec.X * jitter);
                        float vec_y = (yi - y) + (vec.Y * jitter);
                        float vec_z = (zi - z) + (vec.Z * jitter);

                        float new_distance = Math.Abs(vec_x) + Math.Abs(vec_y) + Math.Abs(vec_z);

                        if (new_distance < distance)
                        {
                            distance = new_distance;
                            xc = xi;
                            yc = yi;
                            zc = zi;
                        }
                    }

                    break;
                case CellularDistanceFunction.Natural:
                    for (int xi = xr - 1; xi <= (xr + 1); xi++)
                    for (int yi = yr - 1; yi <= (yr + 1); yi++)
                    for (int zi = zr - 1; zi <= (zr + 1); zi++)
                    {
                        Vector3 vec = Cell3D[Hash3D(seed, xi, yi, zi) & 255];

                        float vec_x = (xi - x) + (vec.X * jitter);
                        float vec_y = (yi - y) + (vec.Y * jitter);
                        float vec_z = (zi - z) + (vec.Z * jitter);

                        float new_distance =
                            Math.Abs(vec_x) + Math.Abs(vec_y) + Math.Abs(vec_z) + ((vec_x * vec_x) + (vec_y * vec_y) + (vec_z * vec_z));

                        if (new_distance < distance)
                        {
                            distance = new_distance;
                            xc = xi;
                            yc = yi;
                            zc = zi;
                        }
                    }

                    break;
                default: throw new ArgumentOutOfRangeException(nameof(distanceFunction), distanceFunction, null);
            }

            switch (returnType)
            {
                case CellularReturnType.CellValue: return ValCoord3D(seed, xc, yc, zc);
                case CellularReturnType.Distance: return distance;
                case CellularReturnType.NoiseLookup: break;
                case CellularReturnType.Distance2: break;
                case CellularReturnType.Distance2Add: break;
                case CellularReturnType.Distance2Sub: break;
                case CellularReturnType.Distance2Mul: break;
                case CellularReturnType.Distance2Div: break;
                default: return 0;
            }

            return 0f;
        }
    }
}
