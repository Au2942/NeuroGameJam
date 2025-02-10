//copy from 2D shuffle by anastadunbar https://www.shadertoy.com/view/MdtXRf
#ifndef SHUFFLED_INCLUDED
#define SHUFFLED_INCLUDED

float rand_float(float co) 
{
    return frac(sin((co)*(91.3458))*47453.5453);
}


float shuffle_float(float x,float div,float i,float seed){
    uint original = floor(x);
    i += seed;
    uint from = uint(floor(rand_float(i*.22)*div));
    uint to = uint(floor(rand_float(i*.82)*div));
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