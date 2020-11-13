# AbstractOTDPlugins
A set of plugins for OpenTabletDriver.

## VectorPredictor
A filter that tries to predict cursor position based on current tangental and normal acceleration.

Works ok on tablets with hardware smoothing, absolutely terrible as of now on Wacoms that have no hw smoothing.

## PathExporter
A passthrough filter that records pen movements to a csv file.
