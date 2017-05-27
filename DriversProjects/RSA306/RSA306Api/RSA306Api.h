// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the RSA306API_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// RSA306API_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef RSA306API_EXPORTS
#define RSA306API_API __declspec(dllexport)
#else
#define RSA306API_API __declspec(dllimport)
#endif

// This class is exported from the RSA306Api.dll
class CRSA306Api {
public:
	CRSA306Api(void);
	int Configure();
	double PeakSearch();
	double PeakValue();
	int Stop();
	int Close();
};


extern "C" 
{
	RSA306API_API int Configure();
	RSA306API_API double PeakSearch();
	RSA306API_API double PeakValue();
}
 
