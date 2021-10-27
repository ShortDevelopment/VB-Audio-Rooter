﻿Imports AudioVisualizer
Imports VBAudioRouter.AudioGraphControl
Imports Windows.Devices.Enumeration
Imports Windows.Media.Audio
Imports Windows.Media.Devices

Namespace Controls

    Public NotInheritable Class SpectrumNodeControl
        Inherits UserControl
        Implements IAudioNodeControl

#Region "Identity"

        Public ReadOnly Property NodeType As NodeTypeEnum Implements IAudioNodeControl.NodeType
            Get
                Return NodeTypeEnum.Output
            End Get
        End Property
        Public Property Canvas As Canvas Implements IAudioNodeControl.Canvas
        Public ReadOnly Property BaseAudioNode As IAudioNode Implements IAudioNodeControl.BaseAudioNode
        Public ReadOnly Property OutgoingConnector As ConnectorControl = Nothing Implements IAudioNodeControl.OutgoingConnector
#End Region

        Public Async Function Initialize(graph As AudioGraph) As Task Implements IAudioNodeControl.Initialize
            _BaseAudioNode = graph.CreateSubmixNode()

            ' https://github.com/clarkezone/audiovisualizer/blob/47938f7cf592daedd705c125b1e218f93d0bbc4b/samples/VisualizationPlayer/AudioNodePage.xaml.cs#L64
            Dim _source As PlaybackSource = PlaybackSource.CreateFromAudioNode(BaseAudioNode)
            Dim _converter As New SourceConverter()
            _converter.Source = _source.Source
            _converter.MinFrequency = 110.0F ' Note A2
            _converter.MaxFrequency = 3520.0F ' Note A7
            _converter.FrequencyCount = 12 * 5 * 5 ' 5 octaves, 5 bars per note
            _converter.FrequencyScale = ScaleType.Logarithmic
            _converter.SpectrumRiseTime = TimeSpan.FromMilliseconds(20)
            _converter.SpectrumFallTime = TimeSpan.FromMilliseconds(200)
            _converter.RmsRiseTime = TimeSpan.FromMilliseconds(20) ' Use RMS To gate noise, fast rise slow fall
            _converter.RmsFallTime = TimeSpan.FromMilliseconds(500)
            _converter.ChannelCount = 1
            SpectrumVisualizer.Source = _converter
        End Function

        Public Sub OnStateChanged(state As GraphState) Implements IAudioNodeControl.OnStateChanged : End Sub
    End Class

End Namespace