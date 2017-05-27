// RSA306ConsoleApp.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <RSA_API.h>
#include <iostream>
#include <Windows.h>
#include <algorithm>


using namespace RSA_API;
using namespace std;

int _tmain(int argc, _TCHAR* argv[])
{

	#define TRACE_LENGTH  801 //*/ 16001
	int numDev; int devID[DEVSRCH_MAX_NUM_DEVICES];
	char  devSN[DEVSRCH_MAX_NUM_DEVICES][DEVSRCH_SERIAL_MAX_STRLEN];
	char  devType[DEVSRCH_MAX_NUM_DEVICES][DEVSRCH_TYPE_MAX_STRLEN];
	// Results returned in user-supplied buffers
	ReturnStatus rs = DEVICE_Search(&numDev, devID, devSN, devType);
	if (rs != noError)
	{
		const char*  err = DEVICE_GetErrorString(rs);
		cout << err << endl;
	}

	if (devID[0] < 0)
	{
		cout << "Failed to find RSA306 device" << endl;
		return 0;
	}
	if (strcmp(devType[0], "RSA306") != 0)
	{
		cout << "Failed to find RSA306 device" << endl;
		return 0;
	}

	rs = DEVICE_Connect(devID[0]);
	if (rs != noError)
	{
		const char*  err = DEVICE_GetErrorString(rs);
		cout << err << endl;
		return 0;
	}


	rs = CONFIG_Preset();
	if (rs != noError)
	{
		const char*  err = DEVICE_GetErrorString(rs);
		cout << err << endl;
		return 0;
	}
	double cf;
	rs = CONFIG_GetCenterFreq(&cf);
	if (rs != noError)
	{
		const char*  err = DEVICE_GetErrorString(rs);
		cout << err << endl;
		return 0;
	}
	cf = 915e6;
	rs = CONFIG_SetCenterFreq(cf);
	if (rs != noError)
	{
		const char*  err = DEVICE_GetErrorString(rs);
		cout << err << endl;
		return 0;
	}

	double refLevel = 0;
	

	rs = CONFIG_GetReferenceLevel(&refLevel);
	if (rs != noError)
	{
		const char*  err = DEVICE_GetErrorString(rs);
		cout << err << endl;
		return 0;
	}

	rs = CONFIG_SetReferenceLevel(refLevel);
	if (rs != noError)
	{
		const char*  err = DEVICE_GetErrorString(rs);
		cout << err << endl;
		return 0;
	}

	bool enable = true;
	rs = SPECTRUM_SetEnable(enable);
	if (rs != noError)
	{
		const char*  err = DEVICE_GetErrorString(rs);
		cout << err << endl;
		return 0;
	}
	rs = SPECTRUM_SetDefault();
	if (rs != noError)
	{
		const char*  err = DEVICE_GetErrorString(rs);
		cout << err << endl;
		return 0;
	}

	Spectrum_Settings spset;
	rs = SPECTRUM_GetSettings(&spset);
	if (rs != noError)
	{
		const char*  err = DEVICE_GetErrorString(rs);
		cout << err << endl;
		return 0;
	}

	bool enableTrace ;
	SpectrumDetectors detector;
	rs = SPECTRUM_GetTraceType(SpectrumTrace1, &enableTrace, &detector);
	if (rs != noError)
	{
		const char*  err = DEVICE_GetErrorString(rs);
		cout << err << endl;
		return 0;
	}
	rs = SPECTRUM_SetTraceType(SpectrumTrace1, enableTrace, SpectrumDetector_AverageVRMS);


	spset.span = 30e6;
	spset.rbw = 300e3;
	//bool enableVBW;
	//double vbw;
	spset.traceLength = TRACE_LENGTH;   //801  MUST be odd number
	//SpectrumWindows window;
	//SpectrumVerticalUnits verticalUnit;

	//  additional settings return from SPECTRUM_GetSettings()
	spset.actualStartFreq = 900e6;
	spset.actualStopFreq = 930e6;
	spset.actualFreqStepSize = 1e6;
	//spset.actualNumIQSamples = 27;
	//double actualRBW;
	//double actualVBW;
	//int actualNumIQSamples;

	
	cout << "Span:" << spset.span;
	cout << "RBW: " << spset.rbw;
	cout << "VBW Enabled: " << spset.enableVBW;
	cout << "VBW: " << spset.vbw;
	cout << "Trace Length: " << spset.traceLength;
	cout << "Window: " + spset.window;
	cout << "Vertical Unit: " << spset.verticalUnit;
	cout << "Actual Start Freq: " << spset.actualStartFreq;
	cout << "Actual End Freq: " << spset.actualStopFreq;
	cout << "Actual Freq Step Size: " << spset.actualFreqStepSize;
	cout << "Actual RBW: " << spset.actualRBW;
	cout << "Actual VBW: " << spset.actualVBW;


	rs = SPECTRUM_SetSettings(spset);
	if (rs != noError)
	{
		const char*  err = DEVICE_GetErrorString(rs);
		cout << err << endl;
		return 0;
	}
	Spectrum_TraceInfo traceInfo;
	int timeoutMsec = 1000;
	bool ready = false;
	DEVICE_Run();
	float traceData[TRACE_LENGTH];
	int outTracePoints;

	double freqStep = (spset.actualStopFreq - spset.actualStartFreq) / TRACE_LENGTH;

	const int N = sizeof(traceData) / sizeof(float);


	float peakFreq;
	while (true)
	{
		SPECTRUM_AcquireTrace();
		SPECTRUM_WaitForTraceReady(timeoutMsec, &ready);
		cout << "Trace Data is Ready" << endl;

		SPECTRUM_GetTrace(SpectrumTrace1, spset.traceLength, traceData, &outTracePoints);

		float maxE =  *std::max_element(traceData, traceData + 801);
		std::cout << "The largest element is " << maxE;

		int MaxFreqLocation = distance(traceData, max_element(traceData, traceData + N));
			
		double value = traceData[MaxFreqLocation];
		cout << "value: " << value;

		peakFreq = spset.actualStartFreq + MaxFreqLocation * freqStep;
		cout << "Peak search: " << peakFreq / 1e6 << endl;
		#if 0 
		SPECTRUM_GetTraceInfo(&traceInfo);
		for (int i = 0; i < 20; i++)
		{
			cout << traceData[i] << " ";
		}
		#endif 
		Sleep(1000);
	}
	DEVICE_Stop();

	rs = DEVICE_Disconnect();
	if (rs != noError)
	{
		const char*  err = DEVICE_GetErrorString(rs);
		cout << err << endl;
		return 0;
	}

	return 0;
}

