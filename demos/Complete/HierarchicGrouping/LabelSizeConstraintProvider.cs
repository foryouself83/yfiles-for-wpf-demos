/****************************************************************************
 ** 
 ** This demo file is part of yFiles WPF 3.4.
 ** Copyright (c) 2000-2021 by yWorks GmbH, Vor dem Kreuzberg 28,
 ** 72070 Tuebingen, Germany. All rights reserved.
 ** 
 ** yFiles demo files exhibit yFiles WPF functionalities. Any redistribution
 ** of demo files in source code or binary form, with or without
 ** modification, is not permitted.
 ** 
 ** Owners of a valid software license for a yFiles WPF version that this
 ** demo is shipped with are allowed to use the demo source code as basis
 ** for their own yFiles WPF powered applications. Use of such programs is
 ** governed by the rights and conditions as set out in the yFiles WPF
 ** license agreement.
 ** 
 ** THIS SOFTWARE IS PROVIDED ''AS IS'' AND ANY EXPRESS OR IMPLIED
 ** WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 ** MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 ** NO EVENT SHALL yWorks BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 ** SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
 ** TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 ** PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 ** LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 ** NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 ** SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ** 
 ***************************************************************************/

using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;

namespace Demo.yFiles.Graph.HierarchicGrouping
{
  /// <summary>
  /// Uses the size constraints provided by the labels to further constrain the size of nodes.
  /// </summary>
  public class LabelSizeConstraintProvider : INodeSizeConstraintProvider 
  {
    private readonly INodeSizeConstraintProvider delegateProvider;

    public LabelSizeConstraintProvider(INodeSizeConstraintProvider delegateProvider) {
      this.delegateProvider = delegateProvider;
    }

    public SizeD GetMinimumSize(INode node) {
      SizeD result = delegateProvider != null ? delegateProvider.GetMinimumSize(node) : SizeD.Empty;
      foreach (ILabel label in node.Labels) {
        INodeSizeConstraintProvider provider = label.LayoutParameter.Model.Lookup<INodeSizeConstraintProvider>();
        if (provider != null) {
          result = SizeD.Max(result, provider.GetMinimumSize(node));
        }
      }
      return result;
    }

    public SizeD GetMaximumSize(INode node) {
      return delegateProvider != null ? delegateProvider.GetMaximumSize(node) : SizeD.Infinite;
    }

    public RectD GetMinimumEnclosedArea(INode node) {
      return delegateProvider != null ? delegateProvider.GetMinimumEnclosedArea(node) : RectD.Empty;
    }
  }
}
