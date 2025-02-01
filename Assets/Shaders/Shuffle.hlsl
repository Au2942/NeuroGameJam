//copy from 2D shuffle by anastadunbar https://www.shadertoy.com/view/MdtXRf
#ifndef SHUFFLED_INCLUDED
#define SHUFFLED_INCLUDED
#define rand(co) frac(sin((co)*(91.3458))*47453.5453)
float shuffle_float(float x,float div,float i,float seed){
    float original = x;
    i += seed;
    float from = float(floor(rand((i)*.22)*div));
    float to = float(floor(rand(i*.82)*div));
    if(original==from){x=to;}
    if(original==to){x=from;}
    return x;
}

void Cell_float(float x, float div, float seed, out float Out)
{
    float a = floor(x*div);
    for(float i=0.;i<70.;i++){
        a = shuffle_float(a,div,i,seed);
    }
    Out = a;
}
#endif // SHUFFLED_INCLUDED